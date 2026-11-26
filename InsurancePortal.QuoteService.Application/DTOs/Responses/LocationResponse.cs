namespace InsurancePortal.QuoteService.Application.DTOs.Responses
{
    /// <summary>
    /// Response DTO for location details
    /// </summary>
    public class LocationResponse
    {
        public Guid Id { get; set; }

        public int? LocationTypeId { get; set; }

        public string? UnitNumber { get; set; }

        public string StreetAddress1 { get; set; }

        public string? StreetAddress2 { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public int ProvinceId { get; set; }

        public int CountryId { get; set; }
    }
}
