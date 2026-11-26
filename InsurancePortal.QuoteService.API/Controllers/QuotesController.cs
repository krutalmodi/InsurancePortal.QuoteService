using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Clients;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.DTOs.Responses;
using InsurancePortal.QuoteService.Application.Lookups;
using InsurancePortal.QuoteService.Application.Quotes.Commands;
using InsurancePortal.QuoteService.Application.Quotes.Queries;
using InsurancePortal.QuoteService.Application.Rating;
using InsurancePortal.QuoteService.Application.Repositories;
using System.Text.Json;
using InsurancePortal.QuoteService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsurancePortal.QuoteService.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/quotes")]
    public class QuotesController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ILogger<QuotesController> _logger;
        private readonly IPolicyServiceClient _policyServiceClient;
        private readonly ILookupRepository _lookupRepository;
        private IReadOnlyList<LookupItem>? _quoteStatuses;

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        public QuotesController(
            IDispatcher dispatcher,
            ILogger<QuotesController> logger,
            IPolicyServiceClient policyServiceClient,
            ILookupRepository lookupRepository)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _policyServiceClient = policyServiceClient ?? throw new ArgumentNullException(nameof(policyServiceClient));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
        }

        /// <summary>
        /// Create a new quote with a single product type
        /// Users do not need to be registered; each request is treated as a guest quote with a generated user ID and reference number
        /// </summary>
        /// <param name="request">Quote request with single product details and PrlooductType</param>
        /// <returns>Created quote with premium calculation</returns>
        [HttpPost]
        public async Task<ActionResult<QuoteResponse>> CreateQuote([FromBody] CreateQuoteRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Quote request cannot be empty");

                var guestUserId = CurrentUserId != null ? Guid.Parse(CurrentUserId) : Guid.NewGuid();
                var quote = await _dispatcher.SendAsync<CreateQuoteCommand, Quote>(new CreateQuoteCommand(guestUserId, request, GetBearerToken()));
                var response = await MapToResponseAsync(quote);

                return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, new
                {
                    message = "Quote created successfully",
                    referenceNumber = quote.ReferenceNumber,
                    quote = response
                });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid argument while creating quote");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error while creating quote");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business logic error while creating quote");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating quote");
                return StatusCode(500, "An error occurred while creating the quote");
            }
        }

        /// <summary>
        /// Get a quote by ID with all product details
        /// </summary>
        /// <param name="id">Quote ID</param>
        /// <returns>Quote details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<QuoteResponse>> GetQuote(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Quote ID is required");

                var quote = await _dispatcher.QueryAsync<GetQuoteByIdQuery, Quote?>(new GetQuoteByIdQuery(id));

                if (quote == null)
                    return NotFound("Quote not found");

                var response = await MapToResponseAsync(quote);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quote");
                return StatusCode(500, "An error occurred while retrieving the quote");
            }
        }

        /// <summary>
        /// Get all quotes for the authenticated user
        /// </summary>
        /// <returns>List of the current user's quotes</returns>
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<QuoteResponse>>> GetUserQuotes()
        {
            if (!Guid.TryParse(CurrentUserId, out var userId) || userId == Guid.Empty)
            {
                _logger.LogWarning("Cannot retrieve user quotes: authenticated token has no valid user identifier");
                return Unauthorized("A valid user identifier is required in the authentication token");
            }

            try
            {
                var quotes = await _dispatcher.QueryAsync<GetUserQuotesQuery, IEnumerable<Quote>>(new GetUserQuotesQuery(userId));
                var responses = new List<QuoteResponse>();

                foreach (var quote in quotes)
                {
                    var response = await MapToResponseAsync(quote);
                    responses.Add(response);
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user quotes");
                return StatusCode(500, "An error occurred while retrieving quotes");
            }
        }

        /// <summary>
        /// Accept a quote and convert it to a policy
        /// </summary>
        /// <param name="id">Quote ID to accept</param>
        /// <returns>Updated quote response</returns>
        [HttpPost("{id}/accept")]
        public async Task<ActionResult<QuoteResponse>> AcceptQuote(Guid id)
        {
            var accepted = false;
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Quote ID is required");
                if (!Guid.TryParse(CurrentUserId, out var currentUserId) || currentUserId == Guid.Empty)
                    return Unauthorized("A valid user identifier is required in the authentication token");

                var quote = await _dispatcher.SendAsync<AcceptQuoteCommand, Quote?>(new AcceptQuoteCommand(id, currentUserId));

                if (quote == null)
                    return NotFound("Quote not found");

                accepted = true;
                var currencies = await _lookupRepository.GetAsync(LookupType.Currencies, includeInactive: true, vehicleMakeId: null);
                var coverages = quote.ProductQuotes.Select(productQuote =>
                {
                    var currency = currencies.SingleOrDefault(item => item.Id == productQuote.CurrencyId)?.IsoCode;
                    if (string.IsNullOrWhiteSpace(currency))
                        throw new InvalidOperationException($"Currency lookup {productQuote.CurrencyId} has no ISO code");

                    return new PolicyCoverageSnapshot(productQuote.ProductType.ToString(), productQuote.QuotedPremium, currency);
                }).ToList();
                var policy = await _policyServiceClient.IssueFromAcceptedQuoteAsync(new PolicyIssueRequest(
                    quote.Id,
                    quote.UserId,
                    quote.ReferenceNumber ?? throw new InvalidOperationException("Quote reference number is missing"),
                    DateTime.UtcNow,
                    coverages));

                var response = await MapToResponseAsync(quote);
                response.PolicyId = policy.Id;
                response.PolicyNumber = policy.PolicyNumber;
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "The authenticated user attempted to accept another user's quote");
                return Forbid();
            }
            catch (QuoteAcceptanceConflictException ex)
            {
                _logger.LogWarning(ex, "Quote acceptance conflicted with an existing status");
                return Conflict(ex.Message);
            }
            catch (PolicyServiceIntegrationException ex)
            {
                _logger.LogError(ex, "Quote was accepted, but Policy Service issuance failed");
                return StatusCode(502, "Quote was accepted, but policy issuance failed. Contact support to reconcile policy issuance; do not retry acceptance.");
            }
            catch (InvalidOperationException ex) when (!accepted)
            {
                _logger.LogWarning(ex, "Cannot accept quote");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting quote; accepted: {Accepted}", accepted);
                return StatusCode(500, accepted
                    ? "Quote was accepted, but a subsequent operation failed. Contact support to reconcile policy issuance; do not retry acceptance."
                    : "An error occurred while accepting the quote");
            }
        }

        /// <summary>
        /// Cancel a quote
        /// </summary>
        /// <param name="id">Quote ID to cancel</param>
        /// <returns>Updated quote response</returns>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<QuoteResponse>> CancelQuote(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Quote ID is required");

                var quote = await _dispatcher.SendAsync<CancelQuoteCommand, Quote?>(new CancelQuoteCommand(id));

                if (quote == null)
                    return NotFound("Quote not found");

                var response = await MapToResponseAsync(quote);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot cancel quote");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling quote");
                return StatusCode(500, "An error occurred while canceling the quote");
            }
        }

        /// <summary>
        /// Get all expired quotes (admin/background job endpoint)
        /// </summary>
        /// <returns>List of expired quotes</returns>
        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<QuoteResponse>>> GetExpiredQuotes()
        {
            try
            {
                var quotes = await _dispatcher.QueryAsync<GetExpiredQuotesQuery, IEnumerable<Quote>>(new GetExpiredQuotesQuery());
                var responses = new List<QuoteResponse>();

                foreach (var quote in quotes)
                {
                    var response = await MapToResponseAsync(quote);
                    responses.Add(response);
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expired quotes");
                return StatusCode(500, "An error occurred while retrieving expired quotes");
            }
        }

        /// <summary>
        /// Mark old quotes as expired (admin/scheduled task endpoint)
        /// </summary>
        [HttpPost("expire-old")]
        public async Task<IActionResult> ExpireOldQuotes()
        {
            try
            {
                await _dispatcher.SendAsync<ExpireOldQuotesCommand, Unit>(new ExpireOldQuotesCommand());
                return Ok(new { message = "Old quotes have been marked as expired" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expiring old quotes");
                return StatusCode(500, "An error occurred while expiring old quotes");
            }
        }

        /// <summary>
        /// Map Quote domain entity to QuoteResponse DTO
        /// </summary>
        private async Task<QuoteResponse> MapToResponseAsync(Quote quote)
        {
            _quoteStatuses ??= await _lookupRepository.GetAsync(LookupType.QuoteStatuses, includeInactive: true, vehicleMakeId: null);
            var status = _quoteStatuses.SingleOrDefault(item => item.Id == quote.StatusId)
                ?? throw new KeyNotFoundException($"Quote status lookup {quote.StatusId} was not found");

            var response = new QuoteResponse
            {
                Id = quote.Id,
                UserId = quote.UserId,
                ReferenceNumber = quote.ReferenceNumber,
                StatusId = quote.StatusId,
                Status = status.Code,
                CreatedAt = quote.CreatedAt,
                ValidUntil = quote.ValidUntil,
                TotalPremium = quote.TotalPremium
            };

            // Map product quotes
            foreach (var productQuote in quote.ProductQuotes)
            {
                switch (productQuote)
                {
                    case HomeQuote homeQuote:
                        {
                            LocationResponse? locationResponse = MapLocation(homeQuote.LocationId, homeQuote.LocationTypeId, homeQuote.UnitNumber, homeQuote.StreetAddress1, homeQuote.StreetAddress2, homeQuote.City, homeQuote.PostalCode, homeQuote.ProvinceId, homeQuote.CountryId);

                            response.HomeQuote = new HomeQuoteResponse
                            {
                                Id = homeQuote.Id,
                                Location = locationResponse,
                                SquareFootage = homeQuote.Details?.SquareFootage ?? 0,
                                YearBuilt = homeQuote.Details?.YearBuilt ?? 0,
                                HomeType = homeQuote.Details?.HomeType,
                                HomeValue = homeQuote.Details?.HomeValue ?? 0,
                                HasSecuritySystem = homeQuote.Details?.HasSecuritySystem ?? false,
                                HasFireAlarm = homeQuote.Details?.HasFireAlarm ?? false,
                                NumberOfOccupants = homeQuote.Details?.NumberOfOccupants ?? 0,
                                QuotedPremium = homeQuote.QuotedPremium,
                                Rating = homeQuote.RatingSnapshot == null ? null : JsonSerializer.Deserialize<RatingResult>(homeQuote.RatingSnapshot),
                                ValidUntil = homeQuote.ValidUntil
                            };
                            break;
                        }

                    case AutoQuote autoQuote:
                        response.AutoQuote = new AutoQuoteResponse
                        {
                            Id = autoQuote.Id,
                            Location = MapLocation(autoQuote.LocationId, autoQuote.LocationTypeId, autoQuote.UnitNumber, autoQuote.StreetAddress1, autoQuote.StreetAddress2, autoQuote.City, autoQuote.PostalCode, autoQuote.ProvinceId, autoQuote.CountryId),
                            VIN = autoQuote.Details?.VIN,
                            VehicleMakeId = autoQuote.Details?.VehicleMakeId ?? 0,
                            VehicleModelId = autoQuote.Details?.VehicleModelId ?? 0,
                            Year = autoQuote.Details?.Year ?? 0,
                            VehicleColorId = autoQuote.Details?.VehicleColorId ?? 0,
                            Mileage = autoQuote.Details?.Mileage ?? 0,
                            DrivingRecord = autoQuote.Details?.DrivingRecord,
                            NumberOfDrivers = autoQuote.Details?.NumberOfDrivers ?? 0,
                            EstimatedAnnualMileage = autoQuote.Details?.EstimatedAnnualMileage ?? 0,
                            VehicleTypeId = autoQuote.VehicleTypeId,
                            AtFaultAccidentCount = autoQuote.AtFaultAccidentCount,
                            SpeedingTicketCount = autoQuote.SpeedingTicketCount,
                            QuotedPremium = autoQuote.QuotedPremium,
                            Rating = autoQuote.RatingSnapshot == null ? null : JsonSerializer.Deserialize<RatingResult>(autoQuote.RatingSnapshot),
                            ValidUntil = autoQuote.ValidUntil
                        };
                        break;

                    case HealthQuote healthQuote:
                        response.HealthQuote = new HealthQuoteResponse
                        {
                            Id = healthQuote.Id,
                            Location = MapLocation(healthQuote.LocationId, healthQuote.LocationTypeId, healthQuote.UnitNumber, healthQuote.StreetAddress1, healthQuote.StreetAddress2, healthQuote.City, healthQuote.PostalCode, healthQuote.ProvinceId, healthQuote.CountryId),
                            Age = healthQuote.Details?.Age ?? 0,
                            HealthStatus = healthQuote.Details?.HealthStatus,
                            HasPreexistingConditions = healthQuote.Details?.HasPreexistingConditions ?? false,
                            Smoker = healthQuote.Details?.Smoker ?? false,
                            CoverageType = healthQuote.Details?.CoverageType,
                            Dependents = healthQuote.Details?.Dependents ?? 0,
                            QuotedPremium = healthQuote.QuotedPremium,
                            ValidUntil = healthQuote.ValidUntil
                        };
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Map a Location from the product's stored address snapshot to LocationResponse
        /// </summary>
        private static LocationResponse? MapLocation(Guid? locationId, int? locationTypeId, string? unitNumber, string? streetAddress1, string? streetAddress2, string? city, string? postalCode, int? provinceId, int? countryId)
        {
            if (!locationId.HasValue)
                return null;

            return new LocationResponse
            {
                Id = locationId.Value,
                LocationTypeId = locationTypeId,
                UnitNumber = unitNumber,
                StreetAddress1 = streetAddress1 ?? string.Empty,
                StreetAddress2 = streetAddress2,
                City = city ?? string.Empty,
                PostalCode = postalCode ?? string.Empty,
                ProvinceId = provinceId ?? 0,
                CountryId = countryId ?? 0
            };
        }

        private string GetBearerToken()
        {
            var header = Request.Headers.Authorization.ToString();
            return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header["Bearer ".Length..] : string.Empty;
        }
    }
}
