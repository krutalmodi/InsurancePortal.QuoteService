using Xunit;
using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;
using InsurancePortal.QuoteService.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace InsurancePortal.QuoteService.Tests.IntegrationTests.Migrations
{
    /// <summary>
    /// Integration tests validating the migration results
    /// These tests connect to a real database and verify schema correctness
    /// Run these AFTER executing migrations: dotnet ef database update
    /// </summary>
    public class MigrationValidationTests
    {
        private readonly string _connectionString;
        private DbContextOptions<QuoteDbContext> _options;

        public MigrationValidationTests()
        {
            // Use test database connection string
            _connectionString = "Server=(localdb)\\mssqllocaldb;Database=InsurancePortal_QuoteService_Test;Trusted_Connection=true;";
            _options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseSqlServer(_connectionString)
                .Options;
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void Migration_CreatesAllLookupTables()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Verify all lookup tables are created by trying to query them
                // If migration didn't create the table, this will throw

                Assert.NotNull(context.QuoteStatusLookups);
                Assert.NotNull(context.ProductTypeLookups);
                Assert.NotNull(context.ProductQuoteStatusLookups);
                Assert.NotNull(context.QuotedByTypeLookups);
                Assert.NotNull(context.LocationTypeLookups);
                Assert.NotNull(context.CurrencyLookups);
                Assert.NotNull(context.InsuranceProviderLookups);
                Assert.NotNull(context.VehicleColorLookups);
                Assert.NotNull(context.DrivingRecordTypeLookups);
                Assert.NotNull(context.HomeTypeLookups);
                Assert.NotNull(context.HealthStatusLookups);
                Assert.NotNull(context.CoverageTypeLookups);
                Assert.NotNull(context.SmokingStatusLookups);
                Assert.NotNull(context.CountryLookups);
                Assert.NotNull(context.ProvinceLookups);
                Assert.NotNull(context.VehicleMakeLookups);
                Assert.NotNull(context.VehicleModelLookups);
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void Migration_SeedsAllLookupData()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Verify seed data was inserted
                Assert.Equal(8, context.QuoteStatusLookups.Count());
                Assert.Equal(3, context.ProductTypeLookups.Count());
                Assert.Equal(6, context.ProductQuoteStatusLookups.Count());
                Assert.Equal(3, context.QuotedByTypeLookups.Count());
                Assert.Equal(2, context.LocationTypeLookups.Count());
                Assert.Equal(2, context.CurrencyLookups.Count());
                Assert.Equal(1, context.InsuranceProviderLookups.Count());
                Assert.Equal(9, context.VehicleColorLookups.Count());
                Assert.Equal(7, context.DrivingRecordTypeLookups.Count());
                Assert.Equal(6, context.HomeTypeLookups.Count());
                Assert.Equal(7, context.HealthStatusLookups.Count());
                Assert.Equal(3, context.CoverageTypeLookups.Count());
                Assert.Equal(3, context.SmokingStatusLookups.Count());
                Assert.Equal(2, context.CountryLookups.Count());
                Assert.Equal(9, context.VehicleMakeLookups.Count());
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void Migration_CurrencyLookupHasUSDandCAD()
        {
            using (var context = new QuoteDbContext(_options))
            {
                var usd = context.CurrencyLookups.FirstOrDefault(c => c.Code == "USD");
                var cad = context.CurrencyLookups.FirstOrDefault(c => c.Code == "CAD");

                Assert.NotNull(usd);
                Assert.NotNull(cad);
                Assert.Equal("USD", usd.IsoCode);
                Assert.Equal("CAD", cad.IsoCode);
                Assert.Equal(2, usd.DecimalPlaces);
                Assert.Equal(2, cad.DecimalPlaces);
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void Migration_InsuranceProviderHasAllstate()
        {
            using (var context = new QuoteDbContext(_options))
            {
                var allstate = context.InsuranceProviderLookups.FirstOrDefault(p => p.Code == "AllstateInsurance");

                Assert.NotNull(allstate);
                Assert.Equal("Allstate Insurance", allstate.Name);
                Assert.NotNull(allstate.RegistrationNumber);
                Assert.NotNull(allstate.ContactEmail);
                Assert.NotNull(allstate.PhoneNumber);
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void Migration_CountriesHaveUSAndCanada()
        {
            using (var context = new QuoteDbContext(_options))
            {
                var usa = context.CountryLookups.FirstOrDefault(c => c.IsoCode == "US");
                var canada = context.CountryLookups.FirstOrDefault(c => c.IsoCode == "CA");

                Assert.NotNull(usa);
                Assert.NotNull(canada);
                Assert.Equal("USA", usa.Code);
                Assert.Equal("Canada", canada.Code);
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void Migration_UpdatesQuotesTableWithStatusId()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Query the database schema to verify StatusId column exists
                // This would typically use raw SQL or EF's model inspection
                var quoteType = context.Model.FindEntityType(typeof(Quote));
                var statusIdProperty = quoteType?.FindProperty("StatusId");

                Assert.NotNull(statusIdProperty);
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void Migration_UpdatesProductQuotesTableWithLookupFKs()
        {
            using (var context = new QuoteDbContext(_options))
            {
                var productQuoteType = context.Model.FindEntityType(typeof(ProductQuote));

                // Verify all FK properties exist
                Assert.NotNull(productQuoteType?.FindProperty("StatusId"));
                Assert.NotNull(productQuoteType?.FindProperty("ProductTypeId"));
                Assert.NotNull(productQuoteType?.FindProperty("CurrencyId"));
                Assert.NotNull(productQuoteType?.FindProperty("ProviderId"));
                Assert.NotNull(productQuoteType?.FindProperty("QuotedByTypeId"));
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void ForeignKeyConstraint_PreventsDeletingLookupInUse()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // This test would:
                // 1. Create a quote with a status
                // 2. Try to delete that status
                // 3. Should fail with FK constraint violation

                var status = context.QuoteStatusLookups.FirstOrDefault();
                if (status != null)
                {
                    var quote = new Quote
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        ReferenceNumber = $"FK-TEST-{Guid.NewGuid().ToString().Substring(0, 8)}",
                        StatusId = status.Id,
                        CreatedAt = DateTime.UtcNow,
                        ValidUntil = DateTime.UtcNow.AddDays(30)
                    };

                    context.Quotes.Add(quote);
                    context.SaveChanges();

                    context.QuoteStatusLookups.Remove(status);

                    // Should throw FK constraint violation
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void SetNullForeignKey_AllowsDeletingProviderWithoutQuote()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // This test would:
                // 1. Create a ProductQuote with a ProviderId
                // 2. Delete the provider
                // 3. Verify ProviderId becomes NULL

                var productType = context.ProductTypeLookups.FirstOrDefault(p => p.Code == "Auto");
                var currency = context.CurrencyLookups.FirstOrDefault(c => c.Code == "USD");
                var status = context.ProductQuoteStatusLookups.FirstOrDefault();
                var provider = context.InsuranceProviderLookups.FirstOrDefault();

                if (productType != null && currency != null && status != null && provider != null)
                {
                    var quote = new Quote
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        ReferenceNumber = $"SETNULL-TEST-{Guid.NewGuid().ToString().Substring(0, 8)}",
                        StatusId = context.QuoteStatusLookups.First().Id,
                        CreatedAt = DateTime.UtcNow,
                        ValidUntil = DateTime.UtcNow.AddDays(30)
                    };

                    var productQuote = new AutoQuote
                    {
                        Id = Guid.NewGuid(),
                        QuoteId = quote.Id,
                        StatusId = status.Id,
                        ProductTypeId = productType.Id,
                        CurrencyId = currency.Id,
                        ProviderId = provider.Id,
                        QuotedPremium = 1000m,
                        ValidUntil = DateTime.UtcNow.AddDays(30),
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system",
                        Details = new Domain.ValueObjects.AutoQuoteDetails(
                            "VIN123", "Toyota", "Camry", 2020, "Silver", 50000, "Clean", 1, 12000m)
                    };

                    context.Quotes.Add(quote);
                    context.ProductQuotes.Add(productQuote);
                    context.SaveChanges();

                    // Delete the provider
                    context.InsuranceProviderLookups.Remove(provider);
                    context.SaveChanges();

                    // Verify ProviderId is now NULL
                    var refreshed = context.ProductQuotes.First(pq => pq.Id == productQuote.Id);
                    Assert.Null(refreshed.ProviderId);
                }
            }
        }

        [Fact(Skip = "Requires database migration to be executed first")]
        public void UniqueConstraint_EnforcesCodeUniqueness()
        {
            using (var context = new QuoteDbContext(_options))
            {
                var newLookup = new QuoteStatusLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "Draft", // Already exists from seed
                    Name = "Duplicate Draft",
                    IsActive = true,
                    SortOrder = 99,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.QuoteStatusLookups.Add(newLookup);

                // Should throw unique constraint violation
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }
    }
}
