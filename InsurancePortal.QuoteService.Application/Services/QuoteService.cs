using InsurancePortal.QuoteService.Application.Clients;
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
        private readonly ILocationClient _locationClient;

        public QuoteService(IQuoteRepository repository, ILocationClient locationClient)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _locationClient = locationClient ?? throw new ArgumentNullException(nameof(locationClient));
        }

        /// <summary>
        /// Create a new quote with a single product type
        /// Each call creates a new quote with exactly one product
        /// </summary>
        public async Task<Quote> CreateQuoteAsync(Guid userId, CreateQuoteRequest request, string accessToken)
        {
            return await CreateQuoteAsync(userId, request, null, accessToken);
        }

        /// <summary>
        /// Create a new quote or add a product to an existing bundled quote
        /// If referenceNumber is provided and exists for this user, adds the product to that quote
        /// Otherwise creates a new quote with a generated reference number
        /// </summary>
        public async Task<Quote> CreateQuoteAsync(Guid userId, CreateQuoteRequest request, string? referenceNumber, string accessToken)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate that the specified product details are provided
            ValidateProductRequest(request);

            Quote quote;

            // If referenceNumber is provided, try to get existing quote
            if (!string.IsNullOrWhiteSpace(referenceNumber))
            {
                var existingQuote = await _repository.GetByReferenceNumberAsync(referenceNumber);
                if (existingQuote != null && existingQuote.UserId == userId)
                {
                    quote = existingQuote;
                    // Validate that we're not adding duplicate product type
                    if (HasProductType(quote, request.ProductType))
                        throw new InvalidOperationException($"Quote already contains a {request.ProductType} product");
                }
                else
                {
                    // Create new quote with provided reference
                    quote = new Quote(userId);
                    quote.ReferenceNumber = referenceNumber;
                }
            }
            else
            {
                // Create new quote with auto-generated reference
                quote = new Quote(userId);
                quote.ReferenceNumber = GenerateReferenceNumber();
            }

            // Add the product based on ProductType
            await AddProductToQuoteAsync(quote, userId, request, accessToken);

            // Validate the quote
            quote.Validate();

            // Save to repository
            if (quote.Id == Guid.Empty)
                await _repository.AddAsync(quote);
            else
                await _repository.UpdateAsync(quote);

            return quote;
        }

        /// <summary>
        /// Validate that the required product details are provided for the specified ProductType
        /// </summary>
        private void ValidateProductRequest(CreateQuoteRequest request)
        {
            switch (request.ProductType)
            {
                case ProductType.Home:
                    if (request.HomeQuote == null)
                        throw new InvalidOperationException("HomeQuote details are required for Home product type");
                    break;
                case ProductType.Auto:
                    if (request.AutoQuote == null)
                        throw new InvalidOperationException("AutoQuote details are required for Auto product type");
                    break;
                case ProductType.Health:
                    if (request.HealthQuote == null)
                        throw new InvalidOperationException("HealthQuote details are required for Health product type");
                    break;
                default:
                    throw new InvalidOperationException($"Unknown product type: {request.ProductType}");
            }
        }

        /// <summary>
        /// Check if quote already contains a product of the specified type
        /// </summary>
        private bool HasProductType(Quote quote, ProductType productType)
        {
            return quote.ProductQuotes.Any(pq => pq.ProductType == productType);
        }

        /// <summary>
        /// Add the appropriate product quote to the quote based on the request ProductType
        /// For Home, Auto and Health quotes, creates a Location entity from the submitted address and stores its ID in the product quote
        /// </summary>
        private async Task AddProductToQuoteAsync(Quote quote, Guid userId, CreateQuoteRequest request, string accessToken)
        {
            var createdBy = userId.ToString();

            if (string.IsNullOrEmpty(quote.CreatedBy))
                quote.CreatedBy = createdBy;

            quote.StatusId = (int)QuoteStatusEnum.Draft;
            switch (request.ProductType)
            {
                case ProductType.Home:
                    {
                        var location = await CreateLocationAsync(request.HomeQuote.Location, accessToken);

                        var homeDetails = new HomeQuoteDetails(
                            request.HomeQuote.SquareFootage,
                            request.HomeQuote.YearBuilt,
                            request.HomeQuote.HomeType,
                            request.HomeQuote.HomeValue,
                            request.HomeQuote.HasSecuritySystem,
                            request.HomeQuote.HasFireAlarm,
                            request.HomeQuote.NumberOfOccupants);

                        var homeQuote = new HomeQuote(homeDetails)
                        {
                            ProductTypeId = (int)ProductType.Home,
                            LocationId = location.Id,
                            UnitNumber = location.UnitNumber,
                            StreetAddress1 = location.StreetAddress1,
                            StreetAddress2 = location.StreetAddress2,
                            City = location.City,
                            PostalCode = location.PostalCode,
                            ProvinceId = location.ProvinceId,
                            CountryId = location.CountryId,
                            CreatedBy = createdBy,
                            StatusId = (int)ProductQuoteStatusEnum.Pending,
                            CurrencyId = location.CountryId == 1 ? (int)CurrencyEnum.USD : (int)CurrencyEnum.CAD // Example logic for currency based on country
                        };
                        quote.AddProductQuote(homeQuote);
                        break;
                    }
                case ProductType.Auto:
                    {
                        var location = await CreateLocationAsync(request.AutoQuote.Location, accessToken);

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

                        var autoQuote = new AutoQuote(autoDetails)
                        {
                            ProductTypeId = (int)ProductType.Auto,
                            LocationId = location.Id,
                            UnitNumber = location.UnitNumber,
                            StreetAddress1 = location.StreetAddress1,
                            StreetAddress2 = location.StreetAddress2,
                            City = location.City,
                            PostalCode = location.PostalCode,
                            ProvinceId = location.ProvinceId,
                            CountryId = location.CountryId,
                            CreatedBy = createdBy,
                            StatusId = (int)ProductQuoteStatusEnum.Pending,
                            CurrencyId = location.CountryId == 1 ? (int)CurrencyEnum.USD : (int)CurrencyEnum.CAD // Example logic for currency based on country
                        };
                        quote.AddProductQuote(autoQuote);
                        break;
                    }
                case ProductType.Health:
                    {
                        var location = await CreateLocationAsync(request.HealthQuote.Location, accessToken);

                        var healthDetails = new HealthQuoteDetails(
                            request.HealthQuote.Age,
                            request.HealthQuote.HealthStatus,
                            request.HealthQuote.HasPreexistingConditions,
                            request.HealthQuote.Smoker,
                            request.HealthQuote.CoverageType,
                            request.HealthQuote.Dependents);

                        var healthQuote = new HealthQuote(healthDetails)
                        {
                            ProductTypeId = (int)ProductType.Health,
                            LocationId = location.Id,
                            UnitNumber = location.UnitNumber,
                            StreetAddress1 = location.StreetAddress1,
                            StreetAddress2 = location.StreetAddress2,
                            City = location.City,
                            PostalCode = location.PostalCode,
                            ProvinceId = location.ProvinceId,
                            CountryId = location.CountryId,
                            CreatedBy = createdBy,
                            StatusId = (int)ProductQuoteStatusEnum.Pending,
                            CurrencyId = location.CountryId == 1 ? (int)CurrencyEnum.USD : (int)CurrencyEnum.CAD // Example logic for currency based on country
                        };
                        quote.AddProductQuote(healthQuote);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Unknown product type: {request.ProductType}");

            }
        }

        /// <summary>
        /// Create and persist a Location from the full address submitted with a quote request
        /// </summary>
        private async Task<LocationDto> CreateLocationAsync(LocationRequest? addressRequest, string accessToken)
        {
            if (addressRequest == null)
                throw new ArgumentNullException(nameof(addressRequest), "Location details are required for this quote");

            return await _locationClient.CreateAsync(addressRequest, accessToken);
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
        /// Get a quote by its reference number
        /// </summary>
        public async Task<Quote?> GetQuoteByReferenceAsync(string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber))
                throw new ArgumentException("Reference number cannot be empty", nameof(referenceNumber));

            return await _repository.GetByReferenceNumberAsync(referenceNumber);
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
