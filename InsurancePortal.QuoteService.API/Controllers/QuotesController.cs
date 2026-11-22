using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.DTOs.Responses;
using InsurancePortal.QuoteService.Application.Quotes.Commands;
using InsurancePortal.QuoteService.Application.Quotes.Queries;
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

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public QuotesController(IDispatcher dispatcher, ILogger<QuotesController> logger)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        /// Get all quotes for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's quotes</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<QuoteResponse>>> GetUserQuotes(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("User ID is required");

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
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Quote ID is required");

                var quote = await _dispatcher.SendAsync<AcceptQuoteCommand, Quote?>(new AcceptQuoteCommand(id));

                if (quote == null)
                    return NotFound("Quote not found");

                var response = await MapToResponseAsync(quote);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot accept quote");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting quote");
                return StatusCode(500, "An error occurred while accepting the quote");
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
            var response = new QuoteResponse
            {
                Id = quote.Id,
                UserId = quote.UserId,
                ReferenceNumber = quote.ReferenceNumber,
                Status = quote.Status.ToString(),
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
                            LocationResponse? locationResponse = MapLocation(homeQuote.LocationId, homeQuote.UnitNumber, homeQuote.StreetAddress1, homeQuote.StreetAddress2, homeQuote.City, homeQuote.PostalCode, homeQuote.ProvinceId, homeQuote.CountryId);

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
                                ValidUntil = homeQuote.ValidUntil
                            };
                            break;
                        }

                    case AutoQuote autoQuote:
                        response.AutoQuote = new AutoQuoteResponse
                        {
                            Id = autoQuote.Id,
                            Location = MapLocation(autoQuote.LocationId, autoQuote.UnitNumber, autoQuote.StreetAddress1, autoQuote.StreetAddress2, autoQuote.City, autoQuote.PostalCode, autoQuote.ProvinceId, autoQuote.CountryId),
                            VIN = autoQuote.Details?.VIN,
                            VehicleMakeId = autoQuote.Details?.VehicleMakeId ?? 0,
                            VehicleModelId = autoQuote.Details?.VehicleModelId ?? 0,
                            Year = autoQuote.Details?.Year ?? 0,
                            VehicleColorId = autoQuote.Details?.VehicleColorId ?? 0,
                            Mileage = autoQuote.Details?.Mileage ?? 0,
                            DrivingRecord = autoQuote.Details?.DrivingRecord,
                            NumberOfDrivers = autoQuote.Details?.NumberOfDrivers ?? 0,
                            EstimatedAnnualMileage = autoQuote.Details?.EstimatedAnnualMileage ?? 0,
                            QuotedPremium = autoQuote.QuotedPremium,
                            ValidUntil = autoQuote.ValidUntil
                        };
                        break;

                    case HealthQuote healthQuote:
                        response.HealthQuote = new HealthQuoteResponse
                        {
                            Id = healthQuote.Id,
                            Location = MapLocation(healthQuote.LocationId, healthQuote.UnitNumber, healthQuote.StreetAddress1, healthQuote.StreetAddress2, healthQuote.City, healthQuote.PostalCode, healthQuote.ProvinceId, healthQuote.CountryId),
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
        private static LocationResponse? MapLocation(Guid? locationId, string? unitNumber, string? streetAddress1, string? streetAddress2, string? city, string? postalCode, int? provinceId, int? countryId)
        {
            if (!locationId.HasValue)
                return null;

            return new LocationResponse
            {
                Id = locationId.Value,
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
