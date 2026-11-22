using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Infrastructure.Data;
using System;

namespace InsurancePortal.QuoteService.Tests.Common
{
    /// <summary>
    /// Test fixture for creating an in-memory QuoteDbContext
    /// Used by unit tests for configuration and mapping validation
    /// </summary>
    public class QuoteDbContextFixture : IDisposable
    {
        private readonly DbContextOptions<QuoteDbContext> _options;
        public QuoteDbContext Context { get; }

        public QuoteDbContextFixture()
        {
            _options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB name per test
                .Options;

            Context = new QuoteDbContext(_options);
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        /// <summary>
        /// Seed basic lookup data for tests
        /// </summary>
        public void SeedLookups()
        {
            // Quote Status Lookup
            var draftStatus = new Domain.Entities.Lookups.QuoteStatusLookup
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Code = "Draft",
                Name = "Draft",
                Description = "Quote is in draft status",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var submittedStatus = new Domain.Entities.Lookups.QuoteStatusLookup
            {
                Id = new Guid("11111111-1111-1111-1111-111111111112"),
                Code = "Submitted",
                Name = "Submitted",
                Description = "Quote has been submitted",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Context.QuoteStatusLookups.AddRange(draftStatus, submittedStatus);

            // Product Type Lookup
            var autoProduct = new Domain.Entities.Lookups.ProductTypeLookup
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Code = "Auto",
                Name = "Auto Insurance",
                Description = "Automobile insurance",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var homeProduct = new Domain.Entities.Lookups.ProductTypeLookup
            {
                Id = new Guid("22222222-2222-2222-2222-222222222223"),
                Code = "Home",
                Name = "Home Insurance",
                Description = "Home insurance",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var healthProduct = new Domain.Entities.Lookups.ProductTypeLookup
            {
                Id = new Guid("22222222-2222-2222-2222-222222222224"),
                Code = "Health",
                Name = "Health Insurance",
                Description = "Health insurance",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Context.ProductTypeLookups.AddRange(autoProduct, homeProduct, healthProduct);

            // Product Quote Status Lookup
            var pendingOfferStatus = new Domain.Entities.Lookups.ProductQuoteStatusLookup
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Code = "Pending",
                Name = "Pending",
                Description = "Offer is pending",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var offeredStatus = new Domain.Entities.Lookups.ProductQuoteStatusLookup
            {
                Id = new Guid("33333333-3333-3333-3333-333333333334"),
                Code = "Offered",
                Name = "Offered",
                Description = "Offer has been made",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Context.ProductQuoteStatusLookups.AddRange(pendingOfferStatus, offeredStatus);

            // Currency Lookup
            var usdCurrency = new Domain.Entities.Lookups.CurrencyLookup
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Code = "USD",
                Name = "US Dollar",
                IsoCode = "USD",
                DecimalPlaces = 2,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var cadCurrency = new Domain.Entities.Lookups.CurrencyLookup
            {
                Id = new Guid("44444444-4444-4444-4444-444444444445"),
                Code = "CAD",
                Name = "Canadian Dollar",
                IsoCode = "CAD",
                DecimalPlaces = 2,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Context.CurrencyLookups.AddRange(usdCurrency, cadCurrency);

            // Insurance Provider Lookup
            var allstateProvider = new Domain.Entities.Lookups.InsuranceProviderLookup
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Code = "AllstateInsurance",
                Name = "Allstate Insurance",
                RegistrationNumber = "REG-00001",
                ContactEmail = "contact@allstate.com",
                PhoneNumber = "1-800-ALLSTATE",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Context.InsuranceProviderLookups.Add(allstateProvider);

            Context.SaveChanges();
        }
    }
}
