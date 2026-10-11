using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Application.Repositories
{
    /// <summary>
    /// Repository interface for Quote aggregate root
    /// </summary>
    public interface IQuoteRepository
    {
        Task<Quote?> GetByIdAsync(Guid id);
        Task<IEnumerable<Quote>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Quote>> GetByStatusAsync(QuoteStatus status);
        Task<IEnumerable<Quote>> GetByProductTypeAsync(ProductType productType);
        Task<Quote?> GetByReferenceNumberAsync(string referenceNumber);
        Task<IEnumerable<Quote>> GetActiveQuotesAsync();
        Task<IEnumerable<Quote>> GetExpiredQuotesAsync();

        Task AddAsync(Quote quote);
        Task UpdateAsync(Quote quote);
        Task DeleteAsync(Guid id);

        Task<bool> ExistsByIdAsync(Guid id);
        Task<int> CountByUserIdAsync(Guid userId);
    }
}
