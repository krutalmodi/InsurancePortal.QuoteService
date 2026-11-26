using System.Text.Json;
using InsurancePortal.QuoteService.Application.Clients;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.DTOs.Responses;
using InsurancePortal.QuoteService.Application.Quotes.Commands;
using InsurancePortal.QuoteService.Application.Rating;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Quotes
{
    public class LocationTypeSnapshotTests
    {
        [Theory]
        [InlineData(ProductType.Auto)]
        [InlineData(ProductType.Home)]
        [InlineData(ProductType.Health)]
        public async Task CreatePersistsLocationTypeReturnedByLocationService(ProductType productType)
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var context = new QuoteDbContext(options);
            var location = new LocationRequest { LocationTypeId = 2 };
            var request = new CreateQuoteRequest
            {
                ProductType = productType,
                AutoQuote = new CreateAutoQuoteRequest
                {
                    Location = location, VIN = "VIN123", VehicleMakeId = 1, VehicleModelId = 1,
                    VehicleColorId = 1, Year = 2020, Mileage = 10000, DrivingRecord = "Clean",
                    NumberOfDrivers = 1, EstimatedAnnualMileage = 10000
                },
                HomeQuote = new CreateHomeQuoteRequest
                {
                    Location = location, SquareFootage = 1500, YearBuilt = 2000, HomeType = "House",
                    HomeValue = 300000, NumberOfOccupants = 1
                },
                HealthQuote = new CreateHealthQuoteRequest
                {
                    Location = location, Age = 35, HealthStatus = "Good", CoverageType = "Individual"
                }
            };
            var repository = new QuoteRepository(context);
            var handler = new CreateQuoteCommandHandler(repository, new LocationClient(), new NoRatingEngine());
            var created = await handler.HandleAsync(new CreateQuoteCommand(Guid.NewGuid(), request, "token"));
            context.ChangeTracker.Clear();

            var saved = await repository.GetByIdAsync(created.Id);
            var product = Assert.Single(saved!.ProductQuotes);
            var typeId = product switch
            {
                AutoQuote auto => auto.LocationTypeId,
                HomeQuote home => home.LocationTypeId,
                HealthQuote health => health.LocationTypeId,
                _ => throw new InvalidOperationException("Unexpected product type")
            };
            Assert.Equal(7, typeId);
        }

        [Fact]
        public void LocationResponseSerializesNumericTypeIdAndNullForHistoricalSnapshots()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var response = new LocationResponse { Id = Guid.NewGuid(), LocationTypeId = 7 };
            using var current = JsonDocument.Parse(JsonSerializer.Serialize(response, options));
            Assert.Equal(7, current.RootElement.GetProperty("locationTypeId").GetInt32());
            response.LocationTypeId = null;
            using var historical = JsonDocument.Parse(JsonSerializer.Serialize(response, options));
            Assert.Equal(JsonValueKind.Null, historical.RootElement.GetProperty("locationTypeId").ValueKind);
            Assert.Equal(typeof(int), typeof(InsurancePortal.QuoteService.Application.Dtos.LocationDto).GetProperty("LocationTypeId")!.PropertyType);
        }

        private sealed class LocationClient : ILocationClient
        {
            public Task<LocationDto> CreateAsync(LocationRequest request, string accessToken) =>
                Task.FromResult(new LocationDto(Guid.NewGuid(), 7, null, "Test", null, "Calgary", "T3J 0A1", 9, 2));

            public Task<LocationDto?> GetByIdAsync(Guid id, string accessToken) => throw new NotSupportedException();
        }

        private sealed class NoRatingEngine : IRatingEngine
        {
            public Task<RatingResult?> RateAsync(ProductQuote quote, AutoRatingInput? autoInput) =>
                Task.FromResult<RatingResult?>(null);
        }
    }
}
