using InsurancePortal.QuoteService.Application.Lookups;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Lookups
{
    public class LookupRepositoryTests
    {
        private static QuoteDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new QuoteDbContext(options);
        }

        [Fact]
        public async Task EveryLookupTypeReadsItsOwnTable()
        {
            using var context = CreateContext();
            context.AddRange(
                new QuoteStatusLookup { Id = 101, Name = "Quote status" },
                new ProductTypeLookup { Id = 102, Name = "Product type" },
                new ProductQuoteStatusLookup { Id = 103, Name = "Product status" },
                new QuotedByTypeLookup { Id = 104, Name = "Quoted by" },
                new VehicleColorLookup { Id = 105, Name = "Color" },
                new DrivingRecordTypeLookup { Id = 106, Name = "Driving record" },
                new HomeTypeLookup { Id = 107, Name = "Home" },
                new HealthStatusLookup { Id = 108, Name = "Health" },
                new CoverageTypeLookup { Id = 109, Name = "Coverage" },
                new SmokingStatusLookup { Id = 110, Name = "Smoking" },
                new CurrencyLookup { Id = 111, Name = "Currency", IsoCode = "CAD", DecimalPlaces = 2 },
                new InsuranceProviderLookup { Id = 112, Name = "Provider" },
                new VehicleMakeLookup { Id = 113, Name = "Make" },
                new VehicleModelLookup { Id = 114, Name = "Model", VehicleMakeId = 113 },
                new VehicleTypeLookup { Id = 115, Name = "Type" });
            await context.SaveChangesAsync();
            var repository = new LookupRepository(context);
            var expected = new[] { 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115 };

            foreach (var type in Enum.GetValues<LookupType>())
            {
                var item = Assert.Single(await repository.GetAsync(type, false, null));
                Assert.Equal(expected[(int)type], item.Id);
                if (type == LookupType.VehicleModels)
                    Assert.Equal(113, item.VehicleMakeId);
                if (type == LookupType.Currencies)
                {
                    Assert.Equal("CAD", item.IsoCode);
                    Assert.Equal(2, item.DecimalPlaces);
                }
            }
        }

        [Fact]
        public async Task FiltersAndOrdersActiveValuesAndCanIncludeInactive()
        {
            using var context = CreateContext();
            context.VehicleColorLookups.AddRange(
                new VehicleColorLookup { Id = 3, Name = "Zed", SortOrder = 1 },
                new VehicleColorLookup { Id = 1, Name = "Alpha", SortOrder = 1 },
                new VehicleColorLookup { Id = 2, Name = "Hidden", SortOrder = 0, IsActive = false });
            await context.SaveChangesAsync();
            var repository = new LookupRepository(context);

            Assert.Equal(new[] { 1, 3 }, (await repository.GetAsync(LookupType.VehicleColors, false, null)).Select(item => item.Id));
            Assert.Equal(new[] { 2, 1, 3 }, (await repository.GetAsync(LookupType.VehicleColors, true, null)).Select(item => item.Id));
        }

        [Fact]
        public async Task VehicleModelsCanBeFilteredByMake()
        {
            using var context = CreateContext();
            context.VehicleModelLookups.AddRange(
                new VehicleModelLookup { Id = 1, Name = "First", VehicleMakeId = 10 },
                new VehicleModelLookup { Id = 2, Name = "Second", VehicleMakeId = 20 });
            await context.SaveChangesAsync();
            var repository = new LookupRepository(context);

            var filtered = await repository.GetAsync(LookupType.VehicleModels, false, 20);
            var item = Assert.Single(filtered);
            Assert.Equal(2, item.Id);
            Assert.Equal(20, item.VehicleMakeId);
            Assert.Empty(await repository.GetAsync(LookupType.VehicleModels, false, 30));
            Assert.Equal(2, (await repository.GetAsync(LookupType.VehicleModels, false, null)).Count);
        }

        [Fact]
        public async Task VehicleTypesExposeSeededValuesAndRespectActiveFlag()
        {
            using var context = CreateContext();
            await context.Database.EnsureCreatedAsync();
            var repository = new LookupRepository(context);
            var items = await repository.GetAsync(LookupType.VehicleTypes, false, null);

            Assert.Equal(Enumerable.Range(1, 11), items.Select(item => item.Id));
            Assert.Equal(new[] { "Sedan", "SUV", "PickupTruck", "Hatchback", "Coupe", "Convertible", "Wagon", "Minivan", "Van", "Motorcycle", "Other" }, items.Select(item => item.Code));
            Assert.All(items, item => Assert.True(item.IsActive));

            var suv = await context.VehicleTypeLookups.FindAsync(2);
            Assert.NotNull(suv);
            suv.IsActive = false;
            await context.SaveChangesAsync();
            Assert.DoesNotContain(await repository.GetAsync(LookupType.VehicleTypes, false, null), item => item.Id == 2);
            Assert.Contains(await repository.GetAsync(LookupType.VehicleTypes, true, null), item => item.Id == 2 && !item.IsActive);
        }

        [Fact]
        public async Task QueryRejectsInvalidTypesAndMakeFilters()
        {
            using var context = CreateContext();
            var handler = new GetLookupsQueryHandler(new LookupRepository(context));

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.HandleAsync(new GetLookupsQuery((LookupType)999, false, null)));
            await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new GetLookupsQuery(LookupType.VehicleModels, false, 0)));
            await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new GetLookupsQuery(LookupType.VehicleMakes, false, 1)));
        }
    }
}
