namespace InsurancePortal.QuoteService.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating an auto insurance quote
    /// </summary>
    public class CreateAutoQuoteRequest
    {
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string DrivingRecord { get; set; }
        public int NumberOfDrivers { get; set; }
        public decimal EstimatedAnnualMileage { get; set; }
    }
}
