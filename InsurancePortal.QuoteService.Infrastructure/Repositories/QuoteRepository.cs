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

        public async Task<IEnumerable<Quote>> GetByStatusAsync(QuoteStatusEnum status)
        {
            return await _context.Quotes
                .Include(q => q.ProductQuotes)
                .Where(q => q.StatusId == (int)status)
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
                .Where(q => (q.StatusId == (int)QuoteStatusEnum.Draft ||
                             q.StatusId == (int)QuoteStatusEnum.Submitted ||
                             q.StatusId == (int)QuoteStatusEnum.InProgress ||
                             q.StatusId == (int)QuoteStatusEnum.Quoted) && DateTime.UtcNow <= q.ValidUntil)
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

        public async Task<bool> TryAcceptAsync(Quote quote)
        {
            ArgumentNullException.ThrowIfNull(quote);
            if (_context.Entry(quote).State == EntityState.Detached)
                throw new InvalidOperationException("Quote must be tracked before acceptance");
            quote.MarkAsAccepted();
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.Entry(quote).State = EntityState.Detached;
                return false;
            }
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
