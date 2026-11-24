using InsurancePortal.QuoteService.Application.Rating;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;
using InsurancePortal.QuoteService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InsurancePortal.QuoteService.Infrastructure.Services
{
    public sealed class BasicRatingOptions
    {
        public bool Enabled { get; set; }
    }

    public sealed class BasicRatingEngine : IRatingEngine
    {
        private const int TermMonths = 12;
        private const int PremiumDecimalPlaces = 2;

        private static class FactorType
        {
            public const string VehicleType = "VehicleType";
            public const string Accidents = "Accidents";
            public const string Tickets = "Tickets";
            public const string Hail = "Hail";
            public const string Theft = "Theft";
            public const string Crash = "Crash";
            public const string Fire = "Fire";
        }

        private readonly QuoteDbContext _context;
        private readonly BasicRatingOptions _options;

        public BasicRatingEngine(QuoteDbContext context, BasicRatingOptions options)
        {
            _context = context;
            _options = options;
        }

        public async Task<RatingResult?> RateAsync(ProductQuote quote, AutoRatingInput? autoInput)
        {
            if (!_options.Enabled || quote is HealthQuote)
                return null;

            var location = GetRatingLocation(quote, autoInput);
            var ratedAt = DateTime.UtcNow;
            var plan = await GetEffectivePlanAsync(quote, location, ratedAt);
            var currency = await GetCurrencyAsync(plan);
            var risk = await GetPostalRiskAsync(plan, location);
            var basePremium = CalculateBasePremium(quote, plan);
            var inputs = GetFactorInputs(quote, autoInput, risk);
            var factors = await GetFactorsAsync(plan.Id);
            var (amount, appliedFactors) = ApplyFactors(basePremium, inputs, factors);

            return new RatingResult(
                plan.Id, plan.Name, risk.DatasetVersion, ratedAt,
                location.CountryId, location.ProvinceId, location.PostalCode,
                currency.IsoCode, TermMonths, plan.BaseRate, (quote as HomeQuote)?.Details?.HomeValue,
                basePremium, appliedFactors, Math.Round(amount, PremiumDecimalPlaces, MidpointRounding.AwayFromZero), currency.Id);
        }

        private static RatingLocation GetRatingLocation(ProductQuote quote, AutoRatingInput? autoInput)
        {
            string? postalCode;
            int? countryId;
            int? provinceId;
            switch (quote)
            {
                case AutoQuote auto:
                    auto.Validate();
                    ValidateAutoInput(autoInput);
                    postalCode = auto.PostalCode;
                    countryId = auto.CountryId;
                    provinceId = auto.ProvinceId;
                    break;
                case HomeQuote home:
                    home.Validate();
                    postalCode = home.PostalCode;
                    countryId = home.CountryId;
                    provinceId = home.ProvinceId;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported product for rating.");
            }

            if (countryId is null or <= 0 || provinceId is null or <= 0 || string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("Country, province and postal code are required for rating.");

            return new RatingLocation(countryId.Value, provinceId.Value, postalCode.ToUpperInvariant());
        }

        private static void ValidateAutoInput(AutoRatingInput? input)
        {
            if (input == null || input.VehicleTypeId < 1 || input.AtFaultAccidents < 0 || input.SpeedingTickets < 0)
                throw new ArgumentException("Vehicle type and non-negative accident and ticket counts are required for rating.");
        }

        private async Task<RatingPlan> GetEffectivePlanAsync(ProductQuote quote, RatingLocation location, DateTime ratedAt)
        {
            var plans = await _context.RatingPlans.AsNoTracking()
                .Where(plan => plan.ProductTypeId == (int)quote.ProductType
                    && plan.CountryId == location.CountryId
                    && plan.ProvinceId == location.ProvinceId
                    && plan.Published
                    && plan.EffectiveFromUtc <= ratedAt
                    && (plan.EffectiveToUtc == null || ratedAt < plan.EffectiveToUtc))
                .ToListAsync();

            if (plans.Count != 1)
                throw new InvalidOperationException("Expected exactly one effective rating plan for the product and jurisdiction.");

            var plan = plans[0];
            if (plan.BaseRate <= 0 || plan.IsPercentageOfHomeValue != (quote is HomeQuote) || plan.RiskDatasetVersion <= 0)
                throw new InvalidOperationException("Invalid rating plan base rate or risk dataset version.");

            return plan;
        }

        private async Task<CurrencyLookup> GetCurrencyAsync(RatingPlan plan)
        {
            var currency = await _context.CurrencyLookups.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == plan.CurrencyId);
            if (currency == null || currency.DecimalPlaces != PremiumDecimalPlaces)
                throw new InvalidOperationException("The rating plan must specify a supported two-decimal currency.");

            return currency;
        }

        private async Task<PostalRisk> GetPostalRiskAsync(RatingPlan plan, RatingLocation location)
        {
            var risk = await _context.PostalRisks.AsNoTracking()
                .SingleOrDefaultAsync(item => item.CountryId == location.CountryId
                    && item.ProvinceId == location.ProvinceId
                    && item.DatasetVersion == plan.RiskDatasetVersion
                    && item.PostalCode == location.PostalCode);
            if (risk == null)
                throw new ArgumentException("No geographic risk data exists for the supplied postal code and rating plan.");

            return risk;
        }

        private static decimal CalculateBasePremium(ProductQuote quote, RatingPlan plan)
        {
            return plan.IsPercentageOfHomeValue
                ? ((HomeQuote)quote).Details!.HomeValue * plan.BaseRate
                : plan.BaseRate;
        }

        private static List<FactorInput> GetFactorInputs(ProductQuote quote, AutoRatingInput? autoInput, PostalRisk risk)
        {
            var inputs = new List<FactorInput>();
            if (quote is AutoQuote)
            {
                inputs.Add(new FactorInput(FactorType.VehicleType, autoInput!.VehicleTypeId));
                inputs.Add(new FactorInput(FactorType.Accidents, autoInput.AtFaultAccidents));
                inputs.Add(new FactorInput(FactorType.Tickets, autoInput.SpeedingTickets));
            }

            inputs.Add(new FactorInput(FactorType.Hail, risk.Hail));
            inputs.Add(new FactorInput(FactorType.Theft, risk.Theft));
            inputs.Add(quote is AutoQuote
                ? new FactorInput(FactorType.Crash, risk.Crash)
                : new FactorInput(FactorType.Fire, risk.Fire));
            return inputs;
        }

        private async Task<List<RatingFactor>> GetFactorsAsync(int planId)
        {
            return await _context.RatingFactors.AsNoTracking()
                .Where(factor => factor.RatingPlanId == planId)
                .ToListAsync();
        }

        private static (decimal Amount, List<AppliedRatingFactor> AppliedFactors) ApplyFactors(
            decimal basePremium, IEnumerable<FactorInput> inputs, IReadOnlyCollection<RatingFactor> factors)
        {
            var amount = basePremium;
            var applied = new List<AppliedRatingFactor>();
            foreach (var input in inputs)
            {
                var factor = FindFactor(input, factors);
                amount *= factor.Multiplier;
                applied.Add(new AppliedRatingFactor(factor.Id, input.Type, input.Value, factor.Multiplier, amount));
            }

            return (amount, applied);
        }

        private static RatingFactor FindFactor(FactorInput input, IReadOnlyCollection<RatingFactor> factors)
        {
            var matches = factors.Where(factor => factor.Type == input.Type
                && factor.Minimum <= input.Value && input.Value <= factor.Maximum).ToList();
            if (matches.Count != 1 || matches[0].Multiplier <= 0)
                throw new InvalidOperationException($"Expected exactly one valid {input.Type} factor for value {input.Value}.");

            return matches[0];
        }

        private sealed record RatingLocation(int CountryId, int ProvinceId, string PostalCode);
        private sealed record FactorInput(string Type, int Value);
    }
}
