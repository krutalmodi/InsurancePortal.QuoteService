using InsurancePortal.QuoteService.Application.Quotes.Commands;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Quotes
{
    public class QuoteAcceptanceConcurrencyTests
    {
        [Fact]
        public async Task OnlyOneStaleReaderCanAcceptAndOtherStatusChangesAreNotOverwritten()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var quote = new Quote(Guid.NewGuid()) { Id = Guid.NewGuid(), ReferenceNumber = "CONCURRENT" };
            using (var setup = new QuoteDbContext(options))
            {
                setup.Quotes.Add(quote);
                await setup.SaveChangesAsync();
            }

            using var first = new QuoteDbContext(options);
            using var second = new QuoteDbContext(options);
            var firstRepository = new QuoteRepository(first);
            var secondRepository = new QuoteRepository(second);
            var firstCopy = (await firstRepository.GetByIdAsync(quote.Id))!;
            var secondCopy = (await secondRepository.GetByIdAsync(quote.Id))!;
            Assert.True(await firstRepository.TryAcceptAsync(firstCopy));
            Assert.False(await secondRepository.TryAcceptAsync(secondCopy));
            using var verify = new QuoteDbContext(options);
            Assert.Equal((int)QuoteStatusEnum.Accepted, (await verify.Quotes.SingleAsync()).StatusId);
        }

        [Fact]
        public async Task StaleCancellationCannotOverwriteAcceptance()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var quote = new Quote(Guid.NewGuid()) { Id = Guid.NewGuid(), ReferenceNumber = "CANCEL-RACE" };
            using (var setup = new QuoteDbContext(options))
            {
                setup.Quotes.Add(quote);
                await setup.SaveChangesAsync();
            }

            using var acceptContext = new QuoteDbContext(options);
            using var cancelContext = new QuoteDbContext(options);
            var accepting = new QuoteRepository(acceptContext);
            var cancelling = new QuoteRepository(cancelContext);
            var acceptCopy = (await accepting.GetByIdAsync(quote.Id))!;
            var cancelCopy = (await cancelling.GetByIdAsync(quote.Id))!;
            Assert.True(await accepting.TryAcceptAsync(acceptCopy));
            cancelCopy.MarkAsExpired();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => cancelling.UpdateAsync(cancelCopy));
            using var verify = new QuoteDbContext(options);
            Assert.Equal((int)QuoteStatusEnum.Accepted, (await verify.Quotes.SingleAsync()).StatusId);
        }

        [Theory]
        [InlineData(QuoteStatusEnum.Accepted)]
        [InlineData(QuoteStatusEnum.Converted)]
        public async Task AlreadyAcceptedOrConvertedQuoteIsRejected(QuoteStatusEnum status)
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var context = new QuoteDbContext(options);
            var quote = new Quote(Guid.NewGuid()) { Id = Guid.NewGuid(), ReferenceNumber = "ACCEPTED", StatusId = (int)status };
            context.Quotes.Add(quote);
            await context.SaveChangesAsync();
            var handler = new AcceptQuoteCommandHandler(new QuoteRepository(context));
            await Assert.ThrowsAsync<QuoteAcceptanceConflictException>(() => handler.HandleAsync(new AcceptQuoteCommand(quote.Id, quote.UserId)));
            Assert.Equal((int)status, (await context.Quotes.SingleAsync()).StatusId);
        }
    }
}
