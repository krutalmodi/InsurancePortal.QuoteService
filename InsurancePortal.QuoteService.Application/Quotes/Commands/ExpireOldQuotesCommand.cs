using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Command to mark old quotes as expired (background job)
    /// </summary>
    public record ExpireOldQuotesCommand() : ICommand<Unit>;
}
