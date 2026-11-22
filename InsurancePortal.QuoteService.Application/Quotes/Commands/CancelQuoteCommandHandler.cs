using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Marks the quote as expired and persists the change
    /// </summary>
    public class CancelQuoteCommandHandler : ICommandHandler<CancelQuoteCommand, Quote?>
    {
        private readonly IQuoteRepository _repository;

        public CancelQuoteCommandHandler(IQuoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Quote?> HandleAsync(CancelQuoteCommand command)
        {
            if (command.QuoteId == Guid.Empty)
                throw new ArgumentException("Quote ID cannot be empty", nameof(command));

            var quote = await _repository.GetByIdAsync(command.QuoteId);
            if (quote == null)
                return null;

            quote.MarkAsExpired();
            await _repository.UpdateAsync(quote);

            return quote;
        }
    }
}
