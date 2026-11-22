using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Command to accept a quote and convert it to a policy
    /// </summary>
    public record AcceptQuoteCommand(Guid QuoteId) : ICommand<Quote?>;
}
