namespace InsurancePortal.QuoteService.Application.DTOs.Responses
{
    /// <summary>
    /// Response DTO for auto insurance quote
    /// </summary>
    public class AutoQuoteResponse
    {
        public Guid Id { get; set; }
        public LocationResponse? Location { get; set; }
        public string VIN { get; set; }
        public int VehicleMakeId { get; set; }
        public int VehicleModelId { get; set; }
        public int Year { get; set; }
        public int VehicleColorId { get; set; }
        public int Mileage { get; set; }
        public string DrivingRecord { get; set; }
        public int NumberOfDrivers { get; set; }
        public decimal EstimatedAnnualMileage { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? AtFaultAccidentCount { get; set; }
        public int? SpeedingTicketCount { get; set; }
        public decimal QuotedPremium { get; set; }
        public InsurancePortal.QuoteService.Application.Rating.RatingResult? Rating { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
