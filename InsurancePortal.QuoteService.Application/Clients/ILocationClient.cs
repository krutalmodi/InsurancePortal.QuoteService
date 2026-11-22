using InsurancePortal.QuoteService.Application.DTOs.Requests;

namespace InsurancePortal.QuoteService.Application.Clients
{
    public interface ILocationClient
    {
        Task<LocationDto?> GetByIdAsync(Guid id, string accessToken);

        Task<LocationDto> CreateAsync(LocationRequest request, string accessToken);
    }

    public record LocationDto(
        Guid Id,
        int LocationTypeId,
        string? UnitNumber,
        string StreetAddress1,
        string? StreetAddress2,
        string City,
        string PostalCode,
        int ProvinceId,
        int CountryId);
}
