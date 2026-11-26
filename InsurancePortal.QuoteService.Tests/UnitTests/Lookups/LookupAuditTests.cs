using InsurancePortal.QuoteService.Domain.Entities.Lookups;
using InsurancePortal.QuoteService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Lookups
{
    public class LookupAuditTests
    {
        [Fact]
        public void AllLookupModelsAndSeedsHaveNullableUpdateAudit()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseSqlServer("Server=localhost;Database=QuoteModel;Trusted_Connection=True;TrustServerCertificate=True").Options;
            using var context = new QuoteDbContext(options);
            var lookups = context.GetService<IDesignTimeModel>().Model.GetEntityTypes()
                .Where(t => typeof(BaseLookup).IsAssignableFrom(t.ClrType)).ToList();
            Assert.Equal(15, lookups.Count);
            foreach (var lookup in lookups)
            {
                Assert.True(lookup.FindProperty(nameof(BaseLookup.UpdatedAt))!.IsNullable);
                Assert.True(lookup.FindProperty(nameof(BaseLookup.UpdatedBy))!.IsNullable);
                Assert.NotEmpty(lookup.GetSeedData());
                foreach (var row in lookup.GetSeedData())
                {
                    Assert.NotNull(row[nameof(BaseLookup.CreatedAt)]);
                    Assert.Equal("seed", row[nameof(BaseLookup.CreatedBy)]);
                    Assert.True(!row.TryGetValue(nameof(BaseLookup.UpdatedAt), out var updatedAt) || updatedAt == null);
                    Assert.True(!row.TryGetValue(nameof(BaseLookup.UpdatedBy), out var updatedBy) || updatedBy == null);
                }
            }
        }

        [Fact]
        public async Task LookupCreationLeavesUpdateAuditNullAndExplicitUpdatesPersist()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var context = new QuoteDbContext(options);
            var lookup = new QuoteStatusLookup { Id = 20, Code = "Test", Name = "Test", CreatedBy = "creator" };
            context.QuoteStatusLookups.Add(lookup);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var saved = await context.QuoteStatusLookups.SingleAsync();
            Assert.Null(saved.UpdatedAt);
            Assert.Null(saved.UpdatedBy);
            var updatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            saved.Name = "Changed";
            saved.UpdatedAt = updatedAt;
            saved.UpdatedBy = "editor";
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            saved = await context.QuoteStatusLookups.SingleAsync();
            Assert.Equal(updatedAt, saved.UpdatedAt);
            Assert.Equal("editor", saved.UpdatedBy);
        }
    }
}
