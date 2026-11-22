using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Command to cancel a quote
    /// </summary>
    public record CancelQuoteCommand(Guid QuoteId) : ICommand<Quote?>;
}
