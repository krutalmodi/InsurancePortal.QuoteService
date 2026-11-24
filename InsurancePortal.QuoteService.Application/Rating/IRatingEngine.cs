using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Rating
{
    public interface IRatingEngine
    {
        Task<RatingResult?> RateAsync(ProductQuote quote, AutoRatingInput? autoInput);
    }

    public record AutoRatingInput(int VehicleTypeId, int AtFaultAccidents, int SpeedingTickets);
    public record AppliedRatingFactor(int FactorId, string Type, int Input, decimal Multiplier, decimal AmountAfter);
    public record RatingResult(int PlanId, string PlanName, int RiskDatasetVersion, DateTime RatedAtUtc,
        int CountryId, int ProvinceId, string PostalCode, string Currency, int TermMonths,
        decimal BaseRate, decimal? HomeValue, decimal BasePremium,
        IReadOnlyList<AppliedRatingFactor> Factors, decimal Premium, int CurrencyId = 0);
}
