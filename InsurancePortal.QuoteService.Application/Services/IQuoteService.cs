using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Services
{
    /// <summary>
    /// Service interface for Quote operations
    /// </summary>
    public interface IQuoteService
    {
        /// <summary>
        /// Create a new quote or add a product to an existing quote with the same reference
        /// Each call creates a quote with exactly one product type
        /// </summary>
        Task<Quote> CreateQuoteAsync(Guid userId, CreateQuoteRequest request, string accessToken);

        /// <summary>
        /// Create a new quote or add a product to an existing bundled quote
        /// If referenceNumber is provided and exists, adds product to that quote
        /// </summary>
        Task<Quote> CreateQuoteAsync(Guid userId, CreateQuoteRequest request, string? referenceNumber, string accessToken);

        Task<Quote?> GetQuoteAsync(Guid quoteId);
        Task<IEnumerable<Quote>> GetUserQuotesAsync(Guid userId);
        Task<Quote?> GetQuoteByReferenceAsync(string referenceNumber);
        Task<Quote?> AcceptQuoteAsync(Guid quoteId);
        Task<Quote?> CancelQuoteAsync(Guid quoteId);
        Task<IEnumerable<Quote>> GetExpiredQuotesAsync();
        Task ExpireOldQuotesAsync();
    }
}
