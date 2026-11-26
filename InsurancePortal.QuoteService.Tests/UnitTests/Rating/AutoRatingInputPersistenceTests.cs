using InsurancePortal.QuoteService.Application.Clients;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.Quotes.Commands;
using InsurancePortal.QuoteService.Application.Rating;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;
using InsurancePortal.QuoteService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Rating
{
    public class AutoRatingInputPersistenceTests
    {
        private static QuoteDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new QuoteDbContext(options);
        }

        private static CreateQuoteRequest Request(int? type, int? accidents, int? tickets) => new()
        {
            ProductType = ProductType.Auto,
            AutoQuote = new CreateAutoQuoteRequest
            {
                Location = new LocationRequest(), VIN = "VIN123", VehicleMakeId = 1,
                VehicleModelId = 1, VehicleColorId = 1, Year = 2020, Mileage = 10000,
                DrivingRecord = "Clean", NumberOfDrivers = 1, EstimatedAnnualMileage = 10000,
                VehicleTypeId = type, AtFaultAccidents = accidents, SpeedingTickets = tickets
            }
        };

        [Theory]
        [InlineData(ProductType.Auto)]
        [InlineData(ProductType.Home)]
        [InlineData(ProductType.Health)]
        public async Task CreateRejectsMissingSelectedProduct(ProductType productType)
        {
            using var context = CreateContext();
            var handler = new CreateQuoteCommandHandler(new QuoteRepository(context), new LocationClient(), new RecordingRatingEngine());
            var request = new CreateQuoteRequest { ProductType = productType };
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), request, "token")));
            Assert.Contains($"{productType}Quote details are required", error.Message);
            Assert.Empty(context.Quotes);
        }

        [Fact]
        public async Task CreatePersistsTheSameValuesPassedToRating()
        {
            using var context = CreateContext();
            var rating = new RecordingRatingEngine();
            var handler = new CreateQuoteCommandHandler(new QuoteRepository(context), new LocationClient(), rating);
            var quote = await handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), Request(2, 0, 3), "token"));
            context.ChangeTracker.Clear();

            var saved = await new QuoteRepository(context).GetByIdAsync(quote.Id);
            var auto = Assert.IsType<AutoQuote>(Assert.Single(saved!.ProductQuotes));
            Assert.Equal(2, auto.VehicleTypeId);
            Assert.Equal(0, auto.AtFaultAccidentCount);
            Assert.Equal(3, auto.SpeedingTicketCount);
            Assert.Equal(new AutoRatingInput(2, 0, 3), rating.Input);
        }

        [Fact]
        public async Task RatedQuotePersistsTheInputsUsedForPremiumAndSnapshot()
        {
            using var context = CreateContext();
            await context.Database.EnsureCreatedAsync();
            context.RatingPlans.Add(new RatingPlan { Id = 1, ProductTypeId = 1, Name = "Test", CountryId = 2, ProvinceId = 9, CurrencyId = 2, RiskDatasetVersion = 1, EffectiveFromUtc = new DateTime(2020, 1, 1), BaseRate = 1000m, Published = true });
            context.PostalRisks.Add(new PostalRisk { Id = 1, CountryId = 2, ProvinceId = 9, DatasetVersion = 1, PostalCode = "T3J 0A1", Hail = 1, Theft = 1, Crash = 1, Fire = 1 });
            context.RatingFactors.AddRange(
                new RatingFactor { Id = 1, RatingPlanId = 1, Type = "VehicleType", Minimum = 2, Maximum = 2, Multiplier = 1.1m },
                new RatingFactor { Id = 2, RatingPlanId = 1, Type = "Accidents", Minimum = 1, Maximum = 1, Multiplier = 1.2m },
                new RatingFactor { Id = 3, RatingPlanId = 1, Type = "Tickets", Minimum = 2, Maximum = 2, Multiplier = 1.3m },
                new RatingFactor { Id = 4, RatingPlanId = 1, Type = "Hail", Minimum = 1, Maximum = 1, Multiplier = 1m },
                new RatingFactor { Id = 5, RatingPlanId = 1, Type = "Theft", Minimum = 1, Maximum = 1, Multiplier = 1m },
                new RatingFactor { Id = 6, RatingPlanId = 1, Type = "Crash", Minimum = 1, Maximum = 1, Multiplier = 1m });
            await context.SaveChangesAsync();
            var engine = new BasicRatingEngine(context, new BasicRatingOptions { Enabled = true });
            var handler = new CreateQuoteCommandHandler(new QuoteRepository(context), new LocationClient(), engine);
            var quote = await handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), Request(2, 1, 2), "token"));
            context.ChangeTracker.Clear();

            var saved = await new QuoteRepository(context).GetByIdAsync(quote.Id);
            var auto = Assert.IsType<AutoQuote>(Assert.Single(saved!.ProductQuotes));
            var snapshot = JsonSerializer.Deserialize<RatingResult>(auto.RatingSnapshot!);
            Assert.NotNull(snapshot);
            Assert.Equal(1716m, saved.TotalPremium);
            Assert.Equal(saved.TotalPremium, auto.QuotedPremium);
            Assert.Equal(auto.VehicleTypeId, snapshot.Factors.Single(f => f.Type == "VehicleType").Input);
            Assert.Equal(auto.AtFaultAccidentCount, snapshot.Factors.Single(f => f.Type == "Accidents").Input);
            Assert.Equal(auto.SpeedingTicketCount, snapshot.Factors.Single(f => f.Type == "Tickets").Input);
        }

        [Fact]
        public async Task OptionalAndPartialInputsRetainNullsWithoutInventingZeros()
        {
            using var context = CreateContext();
            var rating = new RecordingRatingEngine();
            var handler = new CreateQuoteCommandHandler(new QuoteRepository(context), new LocationClient(), rating);
            var quote = await handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), Request(1, null, 0), "token"));
            context.ChangeTracker.Clear();

            var saved = await new QuoteRepository(context).GetByIdAsync(quote.Id);
            var auto = Assert.IsType<AutoQuote>(Assert.Single(saved!.ProductQuotes));
            Assert.Equal(1, auto.VehicleTypeId);
            Assert.Null(auto.AtFaultAccidentCount);
            Assert.Equal(0, auto.SpeedingTicketCount);
            Assert.Null(rating.Input);
        }

        [Fact]
        public async Task EnabledRatingRejectsMissingInputs()
        {
            using var context = CreateContext();
            var engine = new BasicRatingEngine(context, new BasicRatingOptions { Enabled = true });
            var handler = new CreateQuoteCommandHandler(new QuoteRepository(context), new LocationClient(), engine);
            await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), Request(1, null, 0), "token")));
            Assert.Empty(context.Quotes);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, -1, 0)]
        [InlineData(1, 0, -1)]
        public async Task InvalidProvidedInputsAreRejectedEvenWithoutRating(int type, int accidents, int tickets)
        {
            using var context = CreateContext();
            var handler = new CreateQuoteCommandHandler(new QuoteRepository(context), new LocationClient(), new RecordingRatingEngine());
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), Request(type, accidents, tickets), "token")));
            Assert.Empty(context.Quotes);
        }

        [Fact]
        public void RatingInputColumnsAreNullableInSqlServerModel()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseSqlServer("Server=localhost;Database=QuoteModel;Trusted_Connection=True;TrustServerCertificate=True").Options;
            using var context = new QuoteDbContext(options);
            var entity = context.Model.FindEntityType(typeof(AutoQuote))!;
            foreach (var name in new[] { nameof(AutoQuote.VehicleTypeId), nameof(AutoQuote.AtFaultAccidentCount), nameof(AutoQuote.SpeedingTicketCount) })
            {
                var property = entity.FindProperty(name);
                Assert.NotNull(property);
                Assert.True(property.IsNullable);
                Assert.Equal("int", property.GetColumnType());
            }
        }

        private sealed class RecordingRatingEngine : IRatingEngine
        {
            public AutoRatingInput? Input { get; private set; }
            public Task<RatingResult?> RateAsync(ProductQuote quote, AutoRatingInput? autoInput)
            {
                Input = autoInput;
                return Task.FromResult<RatingResult?>(null);
            }
        }

        private sealed class LocationClient : ILocationClient
        {
            public Task<LocationDto> CreateAsync(LocationRequest request, string accessToken) =>
                Task.FromResult(new LocationDto(Guid.NewGuid(), 1, null, "Test", null, "Calgary", "T3J 0A1", 9, 2));

            public Task<LocationDto?> GetByIdAsync(Guid id, string accessToken) => throw new NotSupportedException();
        }
    }
}
