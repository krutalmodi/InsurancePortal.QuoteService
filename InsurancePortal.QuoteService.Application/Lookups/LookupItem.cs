namespace InsurancePortal.QuoteService.Application.Lookups
{
    public record LookupItem(
        int Id,
        string Code,
        string Name,
        string? Description,
        bool IsActive,
        int SortOrder,
        int? VehicleMakeId = null,
        string? IsoCode = null,
        int? DecimalPlaces = null);
}
