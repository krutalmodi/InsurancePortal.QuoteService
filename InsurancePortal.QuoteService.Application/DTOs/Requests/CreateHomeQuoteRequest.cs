namespace InsurancePortal.QuoteService.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating a home insurance quote
    /// </summary>
    public class CreateHomeQuoteRequest
    {
        public string HomeAddress { get; set; }
        public int SquareFootage { get; set; }
        public int YearBuilt { get; set; }
        public string HomeType { get; set; }
        public decimal HomeValue { get; set; }
        public bool HasSecuritySystem { get; set; }
        public bool HasFireAlarm { get; set; }
        public int NumberOfOccupants { get; set; }
    }
}
