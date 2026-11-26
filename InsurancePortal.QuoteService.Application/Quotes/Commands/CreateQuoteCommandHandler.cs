using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Clients;
using InsurancePortal.QuoteService.Application.DTOs.Requests;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Application.Rating;
using System.Text.Json;
using InsurancePortal.QuoteService.Domain.Entities;
using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.ValueObjects;

namespace InsurancePortal.QuoteService.Application.Quotes.Commands
{
    /// <summary>
    /// Handles quote creation, including product validation, LocationService address creation and persistence
    /// </summary>
    public class CreateQuoteCommandHandler : ICommandHandler<CreateQuoteCommand, Quote>
    {
        private readonly IQuoteRepository _repository;
        private readonly ILocationClient _locationClient;
        private readonly IRatingEngine _ratingEngine;

        public CreateQuoteCommandHandler(IQuoteRepository repository, ILocationClient locationClient, IRatingEngine ratingEngine)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _locationClient = locationClient ?? throw new ArgumentNullException(nameof(locationClient));
            _ratingEngine = ratingEngine ?? throw new ArgumentNullException(nameof(ratingEngine));
        }

        public async Task<Quote> HandleAsync(CreateQuoteCommand command)
        {
            if (command.UserId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(command));

            if (command.Request == null)
                throw new ArgumentNullException(nameof(command));

            ValidateProductRequest(command.Request);

            var quote = new Quote(command.UserId)
            {
                ReferenceNumber = GenerateReferenceNumber()
            };

            await AddProductToQuoteAsync(quote, command.UserId, command.Request, command.AccessToken);

            quote.Validate();

            if (quote.Id == Guid.Empty)
                await _repository.AddAsync(quote);
            else
                await _repository.UpdateAsync(quote);

            return quote;
        }

        private static void ValidateProductRequest(CreateQuoteRequest request)
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
                        var homeRequest = request.HomeQuote ?? throw new InvalidOperationException("HomeQuote details are required for Home product type");
                        var location = await CreateLocationAsync(homeRequest.Location, accessToken);

                        var homeDetails = new HomeQuoteDetails(
                            homeRequest.SquareFootage,
                            homeRequest.YearBuilt,
                            homeRequest.HomeType,
                            homeRequest.HomeValue,
                            homeRequest.HasSecuritySystem,
                            homeRequest.HasFireAlarm,
                            homeRequest.NumberOfOccupants);

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
                        await AddRatedProductAsync(quote, homeQuote, null);
                        break;
                    }
                case ProductType.Auto:
                    {
                        var autoRequest = request.AutoQuote ?? throw new InvalidOperationException("AutoQuote details are required for Auto product type");
                        var location = await CreateLocationAsync(autoRequest.Location, accessToken);

                        var autoDetails = new AutoQuoteDetails(
                            autoRequest.VIN,
                            autoRequest.VehicleMakeId,
                            autoRequest.VehicleModelId,
                            autoRequest.Year,
                            autoRequest.VehicleColorId,
                            autoRequest.Mileage,
                            autoRequest.DrivingRecord,
                            autoRequest.NumberOfDrivers,
                            autoRequest.EstimatedAnnualMileage);

                        var autoQuote = new AutoQuote(autoDetails)
                        {
                            ProductTypeId = (int)ProductType.Auto,
                            VehicleTypeId = autoRequest.VehicleTypeId,
                            AtFaultAccidentCount = autoRequest.AtFaultAccidents,
                            SpeedingTicketCount = autoRequest.SpeedingTickets,
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
                        var autoInput = autoQuote.VehicleTypeId.HasValue && autoQuote.AtFaultAccidentCount.HasValue && autoQuote.SpeedingTicketCount.HasValue
                            ? new AutoRatingInput(autoQuote.VehicleTypeId.Value, autoQuote.AtFaultAccidentCount.Value, autoQuote.SpeedingTicketCount.Value)
                            : null;
                        await AddRatedProductAsync(quote, autoQuote, autoInput);
                        break;
                    }
                case ProductType.Health:
                    {
                        var healthRequest = request.HealthQuote ?? throw new InvalidOperationException("HealthQuote details are required for Health product type");
                        var location = await CreateLocationAsync(healthRequest.Location, accessToken);

                        var healthDetails = new HealthQuoteDetails(
                            healthRequest.Age,
                            healthRequest.HealthStatus,
                            healthRequest.HasPreexistingConditions,
                            healthRequest.Smoker,
                            healthRequest.CoverageType,
                            healthRequest.Dependents);

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

        private async Task AddRatedProductAsync(Quote quote, ProductQuote product, AutoRatingInput? autoInput)
        {
            var result = await _ratingEngine.RateAsync(product, autoInput);
            if (result != null)
            {
                product.CurrencyId = result.CurrencyId;
                product.RatingSnapshot = JsonSerializer.Serialize(result);
            }
            quote.AddProductQuote(product, result?.Premium);
        }

        private async Task<LocationDto> CreateLocationAsync(LocationRequest? addressRequest, string accessToken)
        {
            if (addressRequest == null)
                throw new ArgumentNullException(nameof(addressRequest), "Location details are required for this quote");

            return await _locationClient.CreateAsync(addressRequest, accessToken);
        }

        private static string GenerateReferenceNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();
            return $"QT-{date}-{randomPart}";
        }
    }
}
