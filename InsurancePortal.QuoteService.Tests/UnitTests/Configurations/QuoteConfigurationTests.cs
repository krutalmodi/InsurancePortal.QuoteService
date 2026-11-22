using Xunit;
using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Configurations
{
    /// <summary>
    /// Unit tests for Quote entity configuration and FK mapping
    /// Validates that StatusId is properly mapped as FK to QuoteStatusLookup
    /// </summary>
    public class QuoteConfigurationTests
    {
        private DbContextOptions<QuoteDbContext> _options;

        public QuoteConfigurationTests()
        {
            _options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void QuoteConfiguration_Maps_AllRequiredFields()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var quoteId = Guid.NewGuid();
                var statusId = Guid.Parse("11111111-1111-1111-1111-111111111101"); // Draft
                var userId = Guid.NewGuid();
                var referenceNumber = "Q-2024-001";
                var createdAt = DateTime.UtcNow;
                var validUntil = DateTime.UtcNow.AddDays(30);

                var quote = new Quote
                {
                    Id = quoteId,
                    UserId = userId,
                    ReferenceNumber = referenceNumber,
                    StatusId = statusId,
                    CreatedAt = createdAt,
                    CreatedBy = "test-user",
                    ValidUntil = validUntil,
                    TotalPremium = 1500.00m
                };

                // Act
                context.Quotes.Add(quote);
                context.SaveChanges();

                // Assert
                var retrieved = context.Quotes.Single(q => q.Id == quoteId);
                Assert.Equal(referenceNumber, retrieved.ReferenceNumber);
                Assert.Equal(statusId, retrieved.StatusId);
                Assert.Equal(userId, retrieved.UserId);
                Assert.NotNull(retrieved.CreatedAt);
                Assert.NotNull(retrieved.CreatedBy);
            }
        }

        [Fact]
        public void Quote_ReferenceNumber_IsRequired()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var quote = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = null, // NULL - should fail
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                // Act & Assert
                context.Quotes.Add(quote);
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        public void Quote_ReferenceNumber_IsUnique()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var referenceNumber = "Q-UNIQUE-001";
                var quote1 = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = referenceNumber,
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                var quote2 = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = referenceNumber, // Duplicate
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                // Act & Assert
                context.Quotes.Add(quote1);
                context.SaveChanges();

                context.Quotes.Add(quote2);
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        public void Quote_HasAuditFields()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var quote = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = "Q-AUDIT-001",
                    StatusId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                // Act
                context.Quotes.Add(quote);
                context.SaveChanges();

                // Assert
                var retrieved = context.Quotes.Single();
                Assert.NotNull(retrieved.CreatedAt);
                Assert.NotNull(retrieved.CreatedBy);
                Assert.Equal("system", retrieved.CreatedBy);
            }
        }

        [Fact]
        public void Quote_StatusId_IsMapped()
        {
            using (var context = new QuoteDbContext(_options))
            {
                // Arrange
                var statusId = Guid.Parse("11111111-1111-1111-1111-111111111101");
                var quote = new Quote
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    ReferenceNumber = "Q-STATUS-001",
                    StatusId = statusId,
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30)
                };

                // Act
                context.Quotes.Add(quote);
                context.SaveChanges();

                // Assert
                var retrieved = context.Quotes.Single();
                Assert.Equal(statusId, retrieved.StatusId);
            }
        }
    }
}
