using InsurancePortal.QuoteService.Application.Quotes.Queries;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Queries
{
    public class GetUserQuotesQueryTests
    {
        [Fact]
        public async Task ReturnsOnlyQuotesBelongingToRequestedUser()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var context = new QuoteDbContext(options);
            var user = Guid.NewGuid();
            var otherUser = Guid.NewGuid();
            var own = new Quote(user) { Id = Guid.NewGuid(), ReferenceNumber = "OWN" };
            var other = new Quote(otherUser) { Id = Guid.NewGuid(), ReferenceNumber = "OTHER" };
            context.Quotes.AddRange(own, other);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var handler = new GetUserQuotesQueryHandler(new QuoteRepository(context));
            var result = Assert.Single(await handler.HandleAsync(new GetUserQuotesQuery(user)));
            Assert.Equal(own.Id, result.Id);
            Assert.Equal(user, result.UserId);
            Assert.Empty(await handler.HandleAsync(new GetUserQuotesQuery(Guid.NewGuid())));
            await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new GetUserQuotesQuery(Guid.Empty)));
        }
    }
}
