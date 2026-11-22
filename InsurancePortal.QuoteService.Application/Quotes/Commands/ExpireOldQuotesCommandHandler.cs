using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Marks all quotes past their validity as expired
    /// </summary>
    public class ExpireOldQuotesCommandHandler : ICommandHandler<ExpireOldQuotesCommand, Unit>
    {
        private readonly IQuoteRepository _repository;

        public ExpireOldQuotesCommandHandler(IQuoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Unit> HandleAsync(ExpireOldQuotesCommand command)
        {
            var expiredQuotes = await _repository.GetExpiredQuotesAsync();

            foreach (var quote in expiredQuotes.Where(q => q.Status != QuoteStatus.Expired))
            {
                quote.MarkAsExpired();
                await _repository.UpdateAsync(quote);
            }

            return default;
        }
    }
}
