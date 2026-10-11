using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Services
{
    /// <summary>
    /// Service interface for Quote operations
    /// </summary>
    public interface IQuoteService
    {
        Task<Quote> CreateQuoteAsync(Guid userId, CreateQuoteRequest request);
        Task<Quote?> GetQuoteAsync(Guid quoteId);
        Task<IEnumerable<Quote>> GetUserQuotesAsync(Guid userId);
        Task<Quote?> AcceptQuoteAsync(Guid quoteId);
        Task<Quote?> CancelQuoteAsync(Guid quoteId);
        Task<IEnumerable<Quote>> GetExpiredQuotesAsync();
        Task ExpireOldQuotesAsync();
    }
}
