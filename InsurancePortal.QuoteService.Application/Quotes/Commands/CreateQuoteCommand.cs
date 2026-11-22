using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Command to create a new quote with a single product type
    /// </summary>
    public record CreateQuoteCommand(Guid UserId, CreateQuoteRequest Request, string AccessToken) : ICommand<Quote>;
}
