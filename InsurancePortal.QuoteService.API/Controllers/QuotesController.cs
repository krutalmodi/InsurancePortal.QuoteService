using Microsoft.AspNetCore.Mvc;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.DTOs.Responses;
using InsurancePortal.QuoteService.Application.Services;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _quoteService;
        private readonly ILogger<QuotesController> _logger;

        public QuotesController(IQuoteService quoteService, ILogger<QuotesController> logger)
        {
            _quoteService = quoteService ?? throw new ArgumentNullException(nameof(quoteService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new quote with product-specific details
        /// Supports bundled quotes (Home + Auto, etc.)
        /// </summary>
        /// <param name="userId">User ID making the quote request</param>
        /// <param name="request">Quote request with product details</param>
        /// <returns>Created quote with premium calculation</returns>
        [HttpPost]
        public async Task<ActionResult<QuoteResponse>> CreateQuote([FromQuery] Guid userId, [FromBody] CreateQuoteRequest request)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("User ID is required");

                if (request == null)
                    return BadRequest("Quote request cannot be empty");

                var quote = await _quoteService.CreateQuoteAsync(userId, request);
                var response = MapToResponse(quote);

                return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, response);
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

                var quote = await _quoteService.GetQuoteAsync(id);

                if (quote == null)
                    return NotFound("Quote not found");

                return Ok(MapToResponse(quote));
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

                var quotes = await _quoteService.GetUserQuotesAsync(userId);
                var responses = quotes.Select(MapToResponse).ToList();

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

                var quote = await _quoteService.AcceptQuoteAsync(id);

                if (quote == null)
                    return NotFound("Quote not found");

                return Ok(MapToResponse(quote));
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

                var quote = await _quoteService.CancelQuoteAsync(id);

                if (quote == null)
                    return NotFound("Quote not found");

                return Ok(MapToResponse(quote));
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
                var quotes = await _quoteService.GetExpiredQuotesAsync();
                var responses = quotes.Select(MapToResponse).ToList();

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
                await _quoteService.ExpireOldQuotesAsync();
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
        private QuoteResponse MapToResponse(Quote quote)
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
                        response.HomeQuote = new HomeQuoteResponse
                        {
                            Id = homeQuote.Id,
                            HomeAddress = homeQuote.Details?.HomeAddress,
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

                    case AutoQuote autoQuote:
                        response.AutoQuote = new AutoQuoteResponse
                        {
                            Id = autoQuote.Id,
                            VIN = autoQuote.Details?.VIN,
                            Make = autoQuote.Details?.Make,
                            Model = autoQuote.Details?.Model,
                            Year = autoQuote.Details?.Year ?? 0,
                            Color = autoQuote.Details?.Color,
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
    }
}
