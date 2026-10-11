namespace InsurancePortal.QuoteService.Application.DTOs.Responses
{
    /// <summary>
    /// Response DTO for auto insurance quote
    /// </summary>
    public class AutoQuoteResponse
    {
        public Guid Id { get; set; }
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string DrivingRecord { get; set; }
        public int NumberOfDrivers { get; set; }
        public decimal EstimatedAnnualMileage { get; set; }
        public decimal QuotedPremium { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
