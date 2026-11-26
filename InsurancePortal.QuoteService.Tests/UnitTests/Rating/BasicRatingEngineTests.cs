using InsurancePortal.QuoteService.Application.Rating;
using InsurancePortal.QuoteService.Application.Clients;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.Quotes.Commands;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.ValueObjects;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;
using InsurancePortal.QuoteService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Rating
{
    public class BasicRatingEngineTests
    {
        private static QuoteDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new QuoteDbContext(options);
            context.Database.EnsureCreated();
            context.RatingPlans.AddRange(
                new RatingPlan { Id = 1, ProductTypeId = 1, Name = "Auto test plan", CountryId = 2, ProvinceId = 9, CurrencyId = 2, RiskDatasetVersion = 2, EffectiveFromUtc = new DateTime(2020, 1, 1), BaseRate = 1000m, Published = true },
                new RatingPlan { Id = 2, ProductTypeId = 2, Name = "Home test plan", CountryId = 2, ProvinceId = 9, CurrencyId = 2, RiskDatasetVersion = 2, EffectiveFromUtc = new DateTime(2020, 1, 1), BaseRate = 0.005m, IsPercentageOfHomeValue = true, Published = true });
            context.PostalRisks.Add(new PostalRisk { Id = 1, DatasetVersion = 2, CountryId = 2, ProvinceId = 9, PostalCode = "T3J0A1", Hail = 3, Theft = 2, Fire = 1, Crash = 2 });
            context.RatingFactors.AddRange(
                new RatingFactor { Id = 1, RatingPlanId = 1, Type = "VehicleType", Minimum = 1, Maximum = 1, Multiplier = 1m },
                new RatingFactor { Id = 2, RatingPlanId = 1, Type = "VehicleType", Minimum = 2, Maximum = 2, Multiplier = 1.10m },
                new RatingFactor { Id = 4, RatingPlanId = 1, Type = "Accidents", Minimum = 0, Maximum = 0, Multiplier = 1m },
                new RatingFactor { Id = 5, RatingPlanId = 1, Type = "Accidents", Minimum = 1, Maximum = 1, Multiplier = 1.20m },
                new RatingFactor { Id = 6, RatingPlanId = 1, Type = "Accidents", Minimum = 2, Maximum = int.MaxValue, Multiplier = 1.40m },
                new RatingFactor { Id = 7, RatingPlanId = 1, Type = "Tickets", Minimum = 0, Maximum = 0, Multiplier = 1m },
                new RatingFactor { Id = 8, RatingPlanId = 1, Type = "Tickets", Minimum = 1, Maximum = 1, Multiplier = 1.10m },
                new RatingFactor { Id = 9, RatingPlanId = 1, Type = "Tickets", Minimum = 2, Maximum = int.MaxValue, Multiplier = 1.20m },
                new RatingFactor { Id = 12, RatingPlanId = 1, Type = "Hail", Minimum = 3, Maximum = 3, Multiplier = 1.15m },
                new RatingFactor { Id = 14, RatingPlanId = 1, Type = "Theft", Minimum = 2, Maximum = 2, Multiplier = 1.05m },
                new RatingFactor { Id = 17, RatingPlanId = 1, Type = "Crash", Minimum = 2, Maximum = 2, Multiplier = 1.05m },
                new RatingFactor { Id = 21, RatingPlanId = 2, Type = "Hail", Minimum = 3, Maximum = 3, Multiplier = 1.25m },
                new RatingFactor { Id = 23, RatingPlanId = 2, Type = "Theft", Minimum = 2, Maximum = 2, Multiplier = 1.05m },
                new RatingFactor { Id = 25, RatingPlanId = 2, Type = "Fire", Minimum = 1, Maximum = 1, Multiplier = 1m });
            context.SaveChanges();
            return context;
        }

        private static BasicRatingEngine CreateEngine(QuoteDbContext context) =>
            new(context, new BasicRatingOptions { Enabled = true });

        private static AutoQuote Auto(string postalCode = "t3j 0a1", int drivers = 1) => new(
            new AutoQuoteDetails("VIN123", 1, 1, 2022, 1, 10000, "", drivers, 10000))
        { PostalCode = postalCode, CountryId = 2, ProvinceId = 9 };

        private static HomeQuote Home(string postalCode = "T3J0A1") => new(
            new HomeQuoteDetails(2000, 2000, "Detached", 450000m, false, false, 2))
        { PostalCode = postalCode, CountryId = 2, ProvinceId = 9 };

        [Fact]
        public void PostalRisk_PostalCodeColumnSupportsSixteenCharacters()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseSqlServer("Server=localhost;Database=RatingModel;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;
            using var context = new QuoteDbContext(options);
            var property = context.Model.FindEntityType(typeof(PostalRisk))!.FindProperty(nameof(PostalRisk.PostalCode));
            Assert.NotNull(property);
            Assert.Equal(16, property.GetMaxLength());
            Assert.Equal("nvarchar(16)", property.GetColumnType());
        }

        [Fact]
        public async Task Auto_AppliesOrderedFactorsAndRoundsOnlyTheFinalPremium()
        {
            using var context = CreateContext();
            var result = await CreateEngine(context).RateAsync(Auto(), new AutoRatingInput(2, 1, 1));
            Assert.NotNull(result);
            Assert.Equal(1840.95m, result.Premium);
            Assert.Equal("T3J0A1", result.PostalCode);
            Assert.Equal("CAD", result.Currency);
            Assert.Equal(12, result.TermMonths);
            Assert.Equal(new[] { "VehicleType", "Accidents", "Tickets", "Hail", "Theft", "Crash" }, result.Factors.Select(f => f.Type));
            Assert.Equal(1840.9545m, result.Factors.Last().AmountAfter);
        }

        [Fact]
        public async Task Home_UsesSharedRiskWithHomeFactors()
        {
            using var context = CreateContext();
            var result = await CreateEngine(context).RateAsync(Home(), null);
            Assert.NotNull(result);
            Assert.Equal(2250m, result.BasePremium);
            Assert.Equal(2953.13m, result.Premium);
            Assert.Equal(new[] { "Hail", "Theft", "Fire" }, result.Factors.Select(f => f.Type));
        }

        [Theory]
        [InlineData(0, 1.0)]
        [InlineData(1, 1.2)]
        [InlineData(2, 1.4)]
        [InlineData(100, 1.4)]
        public async Task Auto_AccidentBandsMatchBoundaries(int accidents, double multiplier)
        {
            using var context = CreateContext();
            var result = await CreateEngine(context).RateAsync(Auto(), new AutoRatingInput(1, accidents, 0));
            Assert.Equal((decimal)multiplier, result!.Factors.Single(f => f.Type == "Accidents").Multiplier);
        }

        [Fact]
        public async Task InvalidOrUnsupportedInputsDoNotReceiveDefaultRates()
        {
            using var context = CreateContext();
            var engine = CreateEngine(context);
            await Assert.ThrowsAsync<ArgumentException>(() => engine.RateAsync(Auto(), null));
            await Assert.ThrowsAsync<ArgumentException>(() => engine.RateAsync(Auto(), new AutoRatingInput(1, -1, 0)));
            await Assert.ThrowsAsync<ArgumentException>(() => engine.RateAsync(Auto("T2P1J9"), new AutoRatingInput(1, 0, 0)));
            var home = Home();
            home.ProvinceId = 10;
            await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RateAsync(home, null));
        }

        [Fact]
        public async Task OverlappingFactorsFailInsteadOfSelectingOne()
        {
            using var context = CreateContext();
            context.RatingFactors.Add(new RatingFactor { RatingPlanId = 1, Type = "Accidents", Minimum = 3, Maximum = 4, Multiplier = 1.1m });
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateEngine(context).RateAsync(Auto(), new AutoRatingInput(1, 3, 0)));
        }

        [Fact]
        public async Task RatingSnapshotAndPremiumSurvivePersistence()
        {
            using var context = CreateContext();
            var product = Auto();
            product.CreatedBy = "test";
            var result = await CreateEngine(context).RateAsync(product, new AutoRatingInput(2, 1, 1));
            product.RatingSnapshot = System.Text.Json.JsonSerializer.Serialize(result);
            var quote = new Quote(Guid.NewGuid()) { Id = Guid.NewGuid(), CreatedBy = "test", ReferenceNumber = "QT-TEST-1" };
            quote.AddProductQuote(product, result!.Premium);
            context.Quotes.Add(quote);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var saved = await context.Quotes.Include(q => q.ProductQuotes).SingleAsync(q => q.Id == quote.Id);
            var savedProduct = Assert.Single(saved.ProductQuotes);
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<RatingResult>(savedProduct.RatingSnapshot!);
            Assert.Equal(1840.95m, saved.TotalPremium);
            Assert.Equal(saved.TotalPremium, savedProduct.QuotedPremium);
            Assert.Equal(savedProduct.QuotedPremium, snapshot!.Premium);
            Assert.Equal(6, snapshot.Factors.Count);
            saved.MarkAsConverted();
            Assert.Equal((int)QuoteStatusEnum.Converted, saved.StatusId);
        }

        [Fact]
        public async Task CreateQuote_UsesRatingAndPersistsBreakdown()
        {
            using var context = CreateContext();
            var handler = new CreateQuoteCommandHandler(new QuoteRepository(context), new DemoLocationClient(), CreateEngine(context));
            var request = new CreateQuoteRequest
            {
                ProductType = ProductType.Auto,
                AutoQuote = new CreateAutoQuoteRequest
                {
                    Location = new LocationRequest(), VIN = "VIN123", VehicleMakeId = 1, VehicleModelId = 1,
                    VehicleColorId = 1, Year = 2022, Mileage = 10000, DrivingRecord = "",
                    NumberOfDrivers = 1, EstimatedAnnualMileage = 10000,
                    VehicleTypeId = 2, AtFaultAccidents = 1, SpeedingTickets = 1
                }
            };
            var quote = await handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), request, "test"));
            context.ChangeTracker.Clear();
            var saved = await context.Quotes.Include(q => q.ProductQuotes).SingleAsync(q => q.Id == quote.Id);
            var product = Assert.Single(saved.ProductQuotes);
            Assert.Equal(1840.95m, saved.TotalPremium);
            Assert.Equal((int)CurrencyEnum.CAD, product.CurrencyId);
            Assert.Equal(saved.TotalPremium, System.Text.Json.JsonSerializer.Deserialize<RatingResult>(product.RatingSnapshot!)!.Premium);
        }

        private sealed class DemoLocationClient : ILocationClient
        {
            public Task<LocationDto> CreateAsync(LocationRequest request, string accessToken) =>
                Task.FromResult(new LocationDto(Guid.NewGuid(), 1, null, "Test", null, "Calgary", "T3J 0A1", 9, 2));

            public Task<LocationDto?> GetByIdAsync(Guid id, string accessToken) => throw new NotSupportedException();
        }

        [Fact]
        public async Task DisabledEnginePreservesLegacyCalculation()
        {
            using var context = CreateContext();
            var engine = new BasicRatingEngine(context, new BasicRatingOptions());
            Assert.Null(await engine.RateAsync(Auto(), null));
            var product = Auto();
            var quote = new Quote();
            quote.AddProductQuote(product);
            Assert.Equal(product.QuotedPremium, quote.TotalPremium);
        }

        [Fact]
        public async Task MultipleDriversAndConfiguredValuesBeyondFormerLimitsAreRated()
        {
            using var context = CreateContext();
            context.RatingFactors.Add(new RatingFactor { RatingPlanId = 1, Type = "VehicleType", Minimum = 4, Maximum = 4, Multiplier = 1.1m });
            await context.SaveChangesAsync();
            var result = await CreateEngine(context).RateAsync(Auto(drivers: 2), new AutoRatingInput(4, 101, 101));
            Assert.Equal(2, result!.RiskDatasetVersion);
            Assert.Equal(2, result.CurrencyId);
            Assert.Equal(1.40m, result.Factors.Single(f => f.Type == "Accidents").Multiplier);
            Assert.Equal(1.20m, result.Factors.Single(f => f.Type == "Tickets").Multiplier);
        }

        [Fact]
        public async Task WrongJurisdictionOrVersionNeverUsesAnotherPostalRisk()
        {
            using var context = CreateContext();
            var risk = await context.PostalRisks.SingleAsync();
            risk.ProvinceId = 10;
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<ArgumentException>(() => CreateEngine(context).RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
            risk.ProvinceId = 9;
            risk.DatasetVersion = 3;
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<ArgumentException>(() => CreateEngine(context).RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
        }

        [Fact]
        public async Task MissingPlanIsReported()
        {
            using var context = CreateContext();
            foreach (var plan in context.RatingPlans)
                plan.Published = false;
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateEngine(context).RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
        }

        [Fact]
        public async Task MissingOrInvalidFactorIsRejected()
        {
            using var context = CreateContext();
            var factor = await context.RatingFactors.SingleAsync(f => f.Id == 12);
            factor.Multiplier = 0;
            await context.SaveChangesAsync();
            var engine = CreateEngine(context);
            await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
            context.RatingFactors.Remove(factor);
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
        }

        [Fact]
        public async Task InvalidPlanOrCurrencyIsRejected()
        {
            using var context = CreateContext();
            var plan = await context.RatingPlans.SingleAsync(p => p.Id == 1);
            plan.BaseRate = 0;
            await context.SaveChangesAsync();
            var engine = CreateEngine(context);
            await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
            plan.BaseRate = 1000m;
            plan.CurrencyId = 9999;
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
        }

        [Fact]
        public void HistoricalSnapshotWithoutCurrencyIdRemainsReadable()
        {
            const string json = "{\"PlanId\":1,\"PlanName\":\"Old\",\"RiskDatasetVersion\":1,\"RatedAtUtc\":\"2026-01-01T00:00:00Z\",\"CountryId\":2,\"ProvinceId\":9,\"PostalCode\":\"T3J0A1\",\"Currency\":\"CAD\",\"TermMonths\":12,\"BasePremium\":1000,\"BaseRate\":1000,\"Factors\":[],\"Premium\":1000}";
            var snapshot = System.Text.Json.JsonSerializer.Deserialize<RatingResult>(json);
            Assert.Equal("CAD", snapshot!.Currency);
            Assert.Equal(1000m, snapshot.Premium);
            Assert.Equal(0, snapshot.CurrencyId);
        }

        [Fact]
        public async Task OverlappingPlansForSameJurisdictionAreRejected()
        {
            using var context = CreateContext();
            context.RatingPlans.Add(new RatingPlan { ProductTypeId = 1, Name = "Second plan", CountryId = 2, ProvinceId = 9, CurrencyId = 2, RiskDatasetVersion = 2, EffectiveFromUtc = new DateTime(2020, 1, 1), BaseRate = 2000m, Published = true });
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateEngine(context).RateAsync(Auto(), new AutoRatingInput(1, 0, 0)));
        }
    }
}
