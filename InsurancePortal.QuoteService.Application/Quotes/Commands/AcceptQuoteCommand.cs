using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Command to accept a quote before requesting policy issuance
    /// </summary>
    public record AcceptQuoteCommand(Guid QuoteId, Guid CustomerId) : ICommand<Quote?>;
}
