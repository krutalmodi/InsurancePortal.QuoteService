using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Validates and accepts a quote once.
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
            if (quote.UserId != command.CustomerId)
                throw new UnauthorizedAccessException("The quote does not belong to the authenticated user");

            if (quote.StatusId == (int)QuoteStatusEnum.Accepted || quote.StatusId == (int)QuoteStatusEnum.Converted)
                throw new QuoteAcceptanceConflictException("Quote has already been accepted");
            if (quote.IsExpired || quote.StatusId == (int)QuoteStatusEnum.Expired)
                throw new InvalidOperationException("Cannot accept expired quote");
            if (quote.StatusId != (int)QuoteStatusEnum.Draft &&
                quote.StatusId != (int)QuoteStatusEnum.Submitted &&
                quote.StatusId != (int)QuoteStatusEnum.InProgress &&
                quote.StatusId != (int)QuoteStatusEnum.Quoted)
                throw new InvalidOperationException("Cannot accept quote in its current status");

            if (!await _repository.TryAcceptAsync(quote))
                throw new QuoteAcceptanceConflictException("Quote status changed while accepting it");

            return quote;
        }
    }
}
