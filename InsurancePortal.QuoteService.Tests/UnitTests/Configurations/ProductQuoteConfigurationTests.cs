using Xunit;
using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Infrastructure.Data;
using System;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Configurations
{
    /// <summary>
    /// Unit tests for ProductQuote entity configuration
    /// Validates that all lookup FKs (StatusId, ProductTypeId, CurrencyId, ProviderId, QuotedByTypeId) are properly mapped
    /// </summary>
    public class ProductQuoteConfigurationTests
    {
        private DbContextOptions<QuoteDbContext> _options;

        public ProductQuoteConfigurationTests()
        {
            _options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void ProductQuote_Maps_AllLookupForeignKeys()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var quote = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = "Q-FK-TEST-001",
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"), // Draft
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                var productQuote = new AutoQuote
                {
                    Id = Guid.NewGuid(),
                    QuoteId = quote.Id,
                    StatusId = Guid.Parse("33333333-3333-3333-3333-333333333302"), // Offered
                    ProductTypeId = Guid.Parse("22222222-2222-2222-2222-222222222201"), // Auto
                    CurrencyId = Guid.Parse("66666666-6666-6666-6666-666666666601"), // USD
                    ProviderId = Guid.Parse("77777777-7777-7777-7777-777777777701"), // Allstate
                    QuotedByTypeId = Guid.Parse("44444444-4444-4444-4444-444444444402"), // Agent
                    QuotedBy = "John Agent",
                    QuotedPremium = 1500.00m,
                    ValidUntil = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    Details = new Domain.ValueObjects.AutoQuoteDetails(
                        "VIN123456789", 1, 1, 2020, 3,
                        50000, "Clean", 1, 12000m)
                };

                context.Quotes.Add(quote);
                context.ProductQuotes.Add(productQuote);

                // Act
                context.SaveChanges();

                // Assert
                var retrieved = context.AutoQuotes.Single();
                Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333302"), retrieved.StatusId);
                Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222201"), retrieved.ProductTypeId);
                Assert.Equal(Guid.Parse("66666666-6666-6666-6666-666666666601"), retrieved.CurrencyId);
                Assert.Equal(Guid.Parse("77777777-7777-7777-7777-777777777701"), retrieved.ProviderId);
                Assert.Equal(Guid.Parse("44444444-4444-4444-4444-444444444402"), retrieved.QuotedByTypeId);
            }
        }

        [Fact]
        public void ProductQuote_ProviderId_IsOptional()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var quote = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = "Q-OPTIONAL-FK-001",
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                var productQuote = new AutoQuote
                {
                    Id = Guid.NewGuid(),
                    QuoteId = quote.Id,
                    StatusId = Guid.Parse("33333333-3333-3333-3333-333333333301"),
                    ProductTypeId = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                    CurrencyId = Guid.Parse("66666666-6666-6666-6666-666666666601"),
                    ProviderId = null, // Optional - system generated
                    QuotedByTypeId = Guid.Parse("44444444-4444-4444-4444-444444444401"), // System
                    QuotedPremium = 1200.00m,
                    ValidUntil = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    Details = new Domain.ValueObjects.AutoQuoteDetails(
                        "VIN987654321", 1, 1, 2022, 1,
                        25000, "Clean", 1, 15000m)
                };

                context.Quotes.Add(quote);
                context.ProductQuotes.Add(productQuote);

                // Act
                context.SaveChanges();

                // Assert
                var retrieved = context.AutoQuotes.Single();
                Assert.Null(retrieved.ProviderId);
            }
        }

        [Fact]
        public void ProductQuote_HasAuditFields()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var quote = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = "Q-PQ-AUDIT-001",
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                var now = DateTime.UtcNow;
                var productQuote = new AutoQuote
                {
                    Id = Guid.NewGuid(),
                    QuoteId = quote.Id,
                    StatusId = Guid.Parse("33333333-3333-3333-3333-333333333302"),
                    ProductTypeId = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                    CurrencyId = Guid.Parse("66666666-6666-6666-6666-666666666601"),
                    QuotedPremium = 1500.00m,
                    ValidUntil = DateTime.UtcNow.AddDays(30),
                    CreatedAt = now,
                    CreatedBy = "agent@example.com",
                    UpdatedAt = now,
                    UpdatedBy = null,
                    Details = new Domain.ValueObjects.AutoQuoteDetails(
                        "VIN111111111", 1, 1, 2023, 5,
                        5000, "Clean", 2, 20000m)
                };

                context.Quotes.Add(quote);
                context.ProductQuotes.Add(productQuote);

                // Act
                context.SaveChanges();

                // Assert
                var retrieved = context.AutoQuotes.Single();
                Assert.NotNull(retrieved.CreatedAt);
                Assert.NotNull(retrieved.CreatedBy);
                Assert.Equal("agent@example.com", retrieved.CreatedBy);
            }
        }

        [Fact]
        public void ProductQuote_RequiredFKs_AreEnforced()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange - Missing ProductTypeId (required)
                var quote = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = "Q-REQUIRED-FK-001",
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                var productQuote = new AutoQuote
                {
                    Id = Guid.NewGuid(),
                    QuoteId = quote.Id,
                    StatusId = Guid.Parse("33333333-3333-3333-3333-333333333302"),
                    ProductTypeId = null, // Required field - should fail
                    CurrencyId = Guid.Parse("66666666-6666-6666-6666-666666666601"),
                    QuotedPremium = 1500.00m,
                    ValidUntil = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    Details = new Domain.ValueObjects.AutoQuoteDetails(
                        "VIN222222222", 1, 1, 2021, 2,
                        30000, "Minor Violation", 1, 10000m)
                };

                context.Quotes.Add(quote);
                context.ProductQuotes.Add(productQuote);

                // Act & Assert
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }
    }
}
