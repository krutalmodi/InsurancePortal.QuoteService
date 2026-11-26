using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Lookups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsurancePortal.QuoteService.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private static readonly IReadOnlyDictionary<string, LookupType> LookupTypes = new Dictionary<string, LookupType>(StringComparer.OrdinalIgnoreCase)
        {
            ["quote-statuses"] = LookupType.QuoteStatuses,
            ["product-types"] = LookupType.ProductTypes,
            ["product-quote-statuses"] = LookupType.ProductQuoteStatuses,
            ["quoted-by-types"] = LookupType.QuotedByTypes,
            ["vehicle-colors"] = LookupType.VehicleColors,
            ["driving-record-types"] = LookupType.DrivingRecordTypes,
            ["home-types"] = LookupType.HomeTypes,
            ["health-statuses"] = LookupType.HealthStatuses,
            ["coverage-types"] = LookupType.CoverageTypes,
            ["smoking-statuses"] = LookupType.SmokingStatuses,
            ["currencies"] = LookupType.Currencies,
            ["insurance-providers"] = LookupType.InsuranceProviders,
            ["vehicle-makes"] = LookupType.VehicleMakes,
            ["vehicle-models"] = LookupType.VehicleModels,
            ["vehicle-types"] = LookupType.VehicleTypes
        };

        private readonly IDispatcher _dispatcher;
        private readonly ILogger<LookupsController> _logger;

        public LookupsController(IDispatcher dispatcher, ILogger<LookupsController> logger)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{type}")]
        public async Task<ActionResult<IReadOnlyList<LookupItem>>> Get(string type, [FromQuery] bool includeInactive = false, [FromQuery] int? vehicleMakeId = null)
        {
            if (!LookupTypes.TryGetValue(type, out var lookupType))
                return NotFound("Unknown lookup type.");
            if (vehicleMakeId.HasValue && (lookupType != LookupType.VehicleModels || vehicleMakeId <= 0))
                return BadRequest("A positive vehicleMakeId can only be used with vehicle models.");

            try
            {
                var items = await _dispatcher.QueryAsync<GetLookupsQuery, IReadOnlyList<LookupItem>>(
                    new GetLookupsQuery(lookupType, includeInactive, vehicleMakeId));
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookup {LookupType}", type);
                return StatusCode(500, "An error occurred while retrieving the lookup.");
            }
        }
    }
}
