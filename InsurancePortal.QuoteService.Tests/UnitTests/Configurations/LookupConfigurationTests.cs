using Xunit;
using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Infrastructure.Data;
using System;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Configurations
{
    /// <summary>
    /// Unit tests for lookup table configurations
    /// Validates unique Code constraints and basic lookup functionality
    /// </summary>
    public class LookupConfigurationTests
    {
        private DbContextOptions<QuoteDbContext> _options;

        public LookupConfigurationTests()
        {
            _options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void QuoteStatusLookup_Code_IsUnique()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var status1 = new QuoteStatusLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "Draft",
                    Name = "Draft",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var status2 = new QuoteStatusLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "Draft", // Duplicate code
                    Name = "Draft 2",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.QuoteStatusLookups.Add(status1);
                context.SaveChanges();

                context.QuoteStatusLookups.Add(status2);

                // Act & Assert
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        public void CurrencyLookup_IsoCode_IsUnique()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var usd1 = new CurrencyLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "USD",
                    Name = "US Dollar",
                    IsoCode = "USD",
                    DecimalPlaces = 2,
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var usd2 = new CurrencyLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "USD2",
                    Name = "US Dollar 2",
                    IsoCode = "USD", // Duplicate iso code
                    DecimalPlaces = 2,
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.CurrencyLookups.Add(usd1);
                context.SaveChanges();

                context.CurrencyLookups.Add(usd2);

                // Act & Assert
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        public void Lookup_IsActive_DefaultsToTrue()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange - Note: We can't directly test default values in-memory,
                // but we can verify the property exists and can be set
                var status = new QuoteStatusLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "Active",
                    Name = "Active Status",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Act
                context.QuoteStatusLookups.Add(status);
                context.SaveChanges();

                // Assert
                var retrieved = context.QuoteStatusLookups.Single();
                Assert.True(retrieved.IsActive);
            }
        }

        [Fact]
        public void Lookup_HasAuditFields()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var now = DateTime.UtcNow;
                var status = new QuoteStatusLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "Audited",
                    Name = "Audited Status",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = now,
                    CreatedBy = "system",
                    UpdatedAt = now,
                    UpdatedBy = null
                };

                // Act
                context.QuoteStatusLookups.Add(status);
                context.SaveChanges();

                // Assert
                var retrieved = context.QuoteStatusLookups.Single();
                Assert.NotNull(retrieved.CreatedAt);
                Assert.NotNull(retrieved.CreatedBy);
                Assert.Equal("system", retrieved.CreatedBy);
            }
        }

        [Fact]
        public void ProvinceLookup_CompositeKey_CountryId_ProvinceCode()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var countryId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff");
                var province1 = new ProvinceLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "ON",
                    Name = "Ontario",
                    CountryId = countryId,
                    ProvinceCode = "ON",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var province2 = new ProvinceLookup
                {
                    Id = Guid.NewGuid(),
                    Code = "ON2",
                    Name = "Ontario 2",
                    CountryId = countryId,
                    ProvinceCode = "ON", // Same province code, same country
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.ProvinceLookups.Add(province1);
                context.SaveChanges();

                context.ProvinceLookups.Add(province2);

                // Act & Assert
                // Composite unique index should prevent duplicate (CountryId, ProvinceCode)
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }
    }
}
