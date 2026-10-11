using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.ValueObjects;

namespace InsurancePortal.QuoteService.Application.Services
{
    /// <summary>
    /// Service implementation for Quote operations
    /// Handles business logic for quote creation, validation, and conversion
    /// </summary>
    public class QuoteService : IQuoteService
    {
        private readonly IQuoteRepository _repository;

        public QuoteService(IQuoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Create a new quote with product-specific details
        /// Supports bundled quotes (multiple products in one quote)
        /// </summary>
        public async Task<Quote> CreateQuoteAsync(Guid userId, CreateQuoteRequest request)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate that at least one product quote is provided
            if (request.HomeQuote == null && request.AutoQuote == null && request.HealthQuote == null)
                throw new InvalidOperationException("At least one product quote must be provided");

            var quote = new Quote(userId);
            quote.ReferenceNumber = GenerateReferenceNumber();

            // Add Home Quote if provided
            if (request.HomeQuote != null)
            {
                var homeDetails = new HomeQuoteDetails(
                    request.HomeQuote.HomeAddress,
                    request.HomeQuote.SquareFootage,
                    request.HomeQuote.YearBuilt,
                    request.HomeQuote.HomeType,
                    request.HomeQuote.HomeValue,
                    request.HomeQuote.HasSecuritySystem,
                    request.HomeQuote.HasFireAlarm,
                    request.HomeQuote.NumberOfOccupants);

                var homeQuote = new HomeQuote(homeDetails);
                quote.AddProductQuote(homeQuote);
            }

            // Add Auto Quote if provided
            if (request.AutoQuote != null)
            {
                var autoDetails = new AutoQuoteDetails(
                    request.AutoQuote.VIN,
                    request.AutoQuote.Make,
                    request.AutoQuote.Model,
                    request.AutoQuote.Year,
                    request.AutoQuote.Color,
                    request.AutoQuote.Mileage,
                    request.AutoQuote.DrivingRecord,
                    request.AutoQuote.NumberOfDrivers,
                    request.AutoQuote.EstimatedAnnualMileage);

                var autoQuote = new AutoQuote(autoDetails);
                quote.AddProductQuote(autoQuote);
            }

            // Add Health Quote if provided
            if (request.HealthQuote != null)
            {
                var healthDetails = new HealthQuoteDetails(
                    request.HealthQuote.Age,
                    request.HealthQuote.HealthStatus,
                    request.HealthQuote.HasPreexistingConditions,
                    request.HealthQuote.Smoker,
                    request.HealthQuote.CoverageType,
                    request.HealthQuote.Dependents);

                var healthQuote = new HealthQuote(healthDetails);
                quote.AddProductQuote(healthQuote);
            }

            // Validate the complete quote
            quote.Validate();

            // Save to repository
            await _repository.AddAsync(quote);

            return quote;
        }

        /// <summary>
        /// Retrieve a quote by ID
        /// </summary>
        public async Task<Quote?> GetQuoteAsync(Guid quoteId)
        {
            if (quoteId == Guid.Empty)
                throw new ArgumentException("Quote ID cannot be empty", nameof(quoteId));

            return await _repository.GetByIdAsync(quoteId);
        }

        /// <summary>
        /// Get all quotes for a specific user
        /// </summary>
        public async Task<IEnumerable<Quote>> GetUserQuotesAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _repository.GetByUserIdAsync(userId);
        }

        /// <summary>
        /// Accept a quote and mark it as converted to policy
        /// Validates the quote is still active before conversion
        /// </summary>
        public async Task<Quote?> AcceptQuoteAsync(Guid quoteId)
        {
            var quote = await GetQuoteAsync(quoteId);
            if (quote == null)
                return null;

            if (quote.IsExpired)
                throw new InvalidOperationException("Cannot accept expired quote");

            quote.MarkAsConverted();
            await _repository.UpdateAsync(quote);

            return quote;
        }

        /// <summary>
        /// Cancel a quote
        /// </summary>
        public async Task<Quote?> CancelQuoteAsync(Guid quoteId)
        {
            var quote = await GetQuoteAsync(quoteId);
            if (quote == null)
                return null;

            quote.MarkAsExpired();
            await _repository.UpdateAsync(quote);

            return quote;
        }

        /// <summary>
        /// Get all expired quotes
        /// </summary>
        public async Task<IEnumerable<Quote>> GetExpiredQuotesAsync()
        {
            return await _repository.GetExpiredQuotesAsync();
        }

        /// <summary>
        /// Background job to mark old quotes as expired
        /// Can be called periodically or via scheduled task
        /// </summary>
        public async Task ExpireOldQuotesAsync()
        {
            var expiredQuotes = await _repository.GetExpiredQuotesAsync();

            foreach (var quote in expiredQuotes.Where(q => q.Status != QuoteStatus.Expired))
            {
                quote.MarkAsExpired();
                await _repository.UpdateAsync(quote);
            }
        }

        /// <summary>
        /// Generate a unique reference number for the quote
        /// Format: QT-YYYYMMDD-XXXXX (e.g., QT-20261011-ABC12)
        /// </summary>
        private static string GenerateReferenceNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();
            return $"QT-{date}-{randomPart}";
        }
    }
}
