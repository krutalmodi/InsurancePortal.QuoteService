using Microsoft.EntityFrameworkCore;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Infrastructure.Data;

namespace InsurancePortal.QuoteService.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Quote aggregate root
    /// </summary>
    public class QuoteRepository : IQuoteRepository
    {
        private readonly QuoteDbContext _context;

        public QuoteRepository(QuoteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Quote?> GetByIdAsync(Guid id)
        {
            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<Quote>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quote>> GetByStatusAsync(QuoteStatus status)
        {
            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .Where(q => q.Status == status)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quote>> GetByProductTypeAsync(ProductType productType)
        {
            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .Where(q => q.ProductQuotes.Any(pq => pq.ProductType == productType))
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Quote?> GetByReferenceNumberAsync(string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber))
                return null;

            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .FirstOrDefaultAsync(q => q.ReferenceNumber == referenceNumber);
        }

        public async Task<IEnumerable<Quote>> GetActiveQuotesAsync()
        {
            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .Where(q => q.Status == QuoteStatus.Active && DateTime.UtcNow <= q.ValidUntil)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quote>> GetExpiredQuotesAsync()
        {
            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .Where(q => DateTime.UtcNow > q.ValidUntil)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Quote quote)
        {
            if (quote == null)
                throw new ArgumentNullException(nameof(quote));

            await _context.Quotes.AddAsync(quote);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Quote quote)
        {
            if (quote == null)
                throw new ArgumentNullException(nameof(quote));

            _context.Quotes.Update(quote);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var quote = await GetByIdAsync(id);
            if (quote != null)
            {
                _context.Quotes.Remove(quote);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            return await _context.Quotes.AnyAsync(q => q.Id == id);
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Quotes.CountAsync(q => q.UserId == userId);
        }
    }
}
