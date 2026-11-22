using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Validates the quote is still active before marking it as converted
    /// </summary>
    public class AcceptQuoteCommandHandler : ICommandHandler<AcceptQuoteCommand, Quote?>
    {
        private readonly IQuoteRepository _repository;

        public AcceptQuoteCommandHandler(IQuoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Quote?> HandleAsync(AcceptQuoteCommand command)
        {
            if (command.QuoteId == Guid.Empty)
                throw new ArgumentException("Quote ID cannot be empty", nameof(command));

            var quote = await _repository.GetByIdAsync(command.QuoteId);
            if (quote == null)
                return null;

            if (quote.IsExpired)
                throw new InvalidOperationException("Cannot accept expired quote");

            quote.MarkAsConverted();
            await _repository.UpdateAsync(quote);

            return quote;
        }
    }
}
