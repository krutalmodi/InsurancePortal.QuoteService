using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.EventPublishing;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.Events;
using InsurancePortal.QuoteService.Domain.ValueObjects;

namespace InsurancePortal.QuoteService.Application.Services
{
    /// <summary>
    /// Service implementation for Quote operations with event publishing
    /// Handles business logic for quote creation, validation, and conversion
    /// </summary>
    public class QuoteServiceWithEvents : IQuoteService
    {
        private readonly IQuoteRepository _repository;
        private readonly IEventPublisher _eventPublisher;

        public QuoteServiceWithEvents(IQuoteRepository repository, IEventPublisher eventPublisher)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        }

        /// <summary>
        /// Create a new quote with product-specific details and publish QuoteCreatedEvent
        /// </summary>
        public async Task<Quote> CreateQuoteAsync(Guid userId, CreateQuoteRequest request)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.HomeQuote == null && request.AutoQuote == null && request.HealthQuote == null)
                throw new InvalidOperationException("At least one product quote must be provided");

            var quote = new Quote(userId);
            quote.ReferenceNumber = GenerateReferenceNumber();

            // Add product quotes
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

            quote.Validate();

            // Save to repository
            await _repository.AddAsync(quote);

            // Publish QuoteCreatedEvent
            var quoteCreatedEvent = new QuoteCreatedEvent
            {
                QuoteId = quote.Id,
                UserId = quote.UserId,
                ReferenceNumber = quote.ReferenceNumber,
                TotalPremium = quote.TotalPremium,
                Products = quote.GetProductTypes(),
                ValidUntil = quote.ValidUntil
            };

            await _eventPublisher.PublishAsync(quoteCreatedEvent);

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
        /// Accept a quote and publish QuoteAcceptedEvent
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

            // Publish QuoteAcceptedEvent
            var quoteAcceptedEvent = new QuoteAcceptedEvent
            {
                QuoteId = quote.Id,
                UserId = quote.UserId,
                ReferenceNumber = quote.ReferenceNumber,
                TotalPremium = quote.TotalPremium,
                Products = quote.GetProductTypes(),
                CreatedAt = quote.CreatedAt,
                ValidUntil = quote.ValidUntil
            };

            await _eventPublisher.PublishAsync(quoteAcceptedEvent);

            return quote;
        }

        /// <summary>
        /// Cancel a quote and publish QuoteExpiredEvent
        /// </summary>
        public async Task<Quote?> CancelQuoteAsync(Guid quoteId)
        {
            var quote = await GetQuoteAsync(quoteId);
            if (quote == null)
                return null;

            quote.MarkAsExpired();
            await _repository.UpdateAsync(quote);

            // Publish QuoteExpiredEvent
            var quoteExpiredEvent = new QuoteExpiredEvent
            {
                QuoteId = quote.Id,
                UserId = quote.UserId,
                ReferenceNumber = quote.ReferenceNumber,
                ValidUntil = quote.ValidUntil
            };

            await _eventPublisher.PublishAsync(quoteExpiredEvent);

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
        /// Mark old quotes as expired and publish events
        /// </summary>
        public async Task ExpireOldQuotesAsync()
        {
            var expiredQuotes = await _repository.GetExpiredQuotesAsync();
            var events = new List<DomainEvent>();

            foreach (var quote in expiredQuotes.Where(q => q.Status != QuoteStatus.Expired))
            {
                quote.MarkAsExpired();
                await _repository.UpdateAsync(quote);

                var quoteExpiredEvent = new QuoteExpiredEvent
                {
                    QuoteId = quote.Id,
                    UserId = quote.UserId,
                    ReferenceNumber = quote.ReferenceNumber,
                    ValidUntil = quote.ValidUntil
                };

                events.Add(quoteExpiredEvent);
            }

            if (events.Any())
            {
                await _eventPublisher.PublishBatchAsync(events);
            }
        }

        private static string GenerateReferenceNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();
            return $"QT-{date}-{randomPart}";
        }
    }
}
