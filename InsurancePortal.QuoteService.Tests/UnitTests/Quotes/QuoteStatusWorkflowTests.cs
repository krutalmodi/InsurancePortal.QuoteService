using InsurancePortal.QuoteService.Application.Quotes.Commands;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Infrastructure.Data;
using InsurancePortal.QuoteService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsurancePortal.QuoteService.Tests.UnitTests.Quotes
{
    public class QuoteStatusWorkflowTests
    {
        private static QuoteDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<QuoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new QuoteDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task StatusIdsMatchSeededLookupsAndObsoletePropertyIsNotMapped()
        {
            using var context = CreateContext();
            var statuses = await context.QuoteStatusLookups.ToDictionaryAsync(x => x.Id, x => x.Code);
            foreach (var status in Enum.GetValues<QuoteStatusEnum>())
                Assert.Equal(status.ToString(), statuses[(int)status]);

            var quoteType = context.Model.FindEntityType(typeof(Quote))!;
            Assert.Null(quoteType.FindProperty("Status"));
            Assert.NotNull(quoteType.FindProperty("StatusId"));
            Assert.Equal(1, new Quote().StatusId);
        }

        [Fact]
        public async Task AcceptPersistsAcceptedStatusAndRejectsRepeat()
        {
            using var context = CreateContext();
            var quote = new Quote(Guid.NewGuid()) { Id = Guid.NewGuid(), ReferenceNumber = "ACCEPT" };
            context.Quotes.Add(quote);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var handler = new AcceptQuoteCommandHandler(new QuoteRepository(context));

            var accepted = await handler.HandleAsync(new AcceptQuoteCommand(quote.Id, quote.UserId));
            Assert.Equal(6, accepted!.StatusId);
            context.ChangeTracker.Clear();
            var saved = await context.Quotes.SingleAsync(q => q.Id == quote.Id);
            Assert.Equal(6, saved.StatusId);
            saved.ValidUntil = DateTime.UtcNow.AddDays(-1);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            await Assert.ThrowsAsync<QuoteAcceptanceConflictException>(() => handler.HandleAsync(new AcceptQuoteCommand(quote.Id, quote.UserId)));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(new AcceptQuoteCommand(quote.Id, Guid.NewGuid())));
        }

        [Theory]
        [InlineData(QuoteStatusEnum.Expired)]
        [InlineData(QuoteStatusEnum.Declined)]
        public async Task AcceptRejectsTerminalStatuses(QuoteStatusEnum status)
        {
            using var context = CreateContext();
            var quote = new Quote(Guid.NewGuid()) { Id = Guid.NewGuid(), ReferenceNumber = "TERMINAL", StatusId = (int)status };
            context.Quotes.Add(quote);
            await context.SaveChangesAsync();
            var handler = new AcceptQuoteCommandHandler(new QuoteRepository(context));
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new AcceptQuoteCommand(quote.Id, quote.UserId)));
            Assert.Equal((int)status, (await context.Quotes.FindAsync(quote.Id))!.StatusId);
        }

        [Fact]
        public async Task AcceptRejectsExpiredOpenQuote()
        {
            using var context = CreateContext();
            var quote = new Quote(Guid.NewGuid()) { Id = Guid.NewGuid(), ReferenceNumber = "EXPIRED", ValidUntil = DateTime.UtcNow.AddDays(-1) };
            context.Quotes.Add(quote);
            await context.SaveChangesAsync();
            var handler = new AcceptQuoteCommandHandler(new QuoteRepository(context));
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new AcceptQuoteCommand(quote.Id, quote.UserId)));
            Assert.Equal(1, (await context.Quotes.FindAsync(quote.Id))!.StatusId);
        }

        [Fact]
        public async Task ExpirationAndCancellationPreserveAcceptedQuotes()
        {
            using var context = CreateContext();
            var user = Guid.NewGuid();
            var oldDraft = new Quote(user) { Id = Guid.NewGuid(), ReferenceNumber = "OLD", ValidUntil = DateTime.UtcNow.AddDays(-1) };
            var accepted = new Quote(user) { Id = Guid.NewGuid(), ReferenceNumber = "ACCEPTED", StatusId = 6, ValidUntil = DateTime.UtcNow.AddDays(-1) };
            var converted = new Quote(user) { Id = Guid.NewGuid(), ReferenceNumber = "CONVERTED", StatusId = 7, ValidUntil = DateTime.UtcNow.AddDays(-1) };
            var active = new Quote(user) { Id = Guid.NewGuid(), ReferenceNumber = "ACTIVE", StatusId = 4 };
            context.Quotes.AddRange(oldDraft, accepted, converted, active);
            await context.SaveChangesAsync();
            var repository = new QuoteRepository(context);
            Assert.Equal(active.Id, Assert.Single(await repository.GetActiveQuotesAsync()).Id);
            await new ExpireOldQuotesCommandHandler(repository).HandleAsync(new ExpireOldQuotesCommand());
            context.ChangeTracker.Clear();
            Assert.Equal(5, (await context.Quotes.FindAsync(oldDraft.Id))!.StatusId);
            Assert.Equal(6, (await context.Quotes.FindAsync(accepted.Id))!.StatusId);
            Assert.Equal(7, (await context.Quotes.FindAsync(converted.Id))!.StatusId);
            var cancel = new CancelQuoteCommandHandler(repository);
            await Assert.ThrowsAsync<InvalidOperationException>(() => cancel.HandleAsync(new CancelQuoteCommand(accepted.Id)));
            await Assert.ThrowsAsync<InvalidOperationException>(() => cancel.HandleAsync(new CancelQuoteCommand(converted.Id)));
            var cancelled = await cancel.HandleAsync(new CancelQuoteCommand(active.Id));
            Assert.Equal(5, cancelled!.StatusId);
        }
    }
}
