namespace InsurancePortal.QuoteService.Application.DTOs.Responses
{
    /// <summary>
    /// Response DTO for home insurance quote
    /// </summary>
    public class HomeQuoteResponse
    {
        public Guid Id { get; set; }
        public string HomeAddress { get; set; }
        public int SquareFootage { get; set; }
        public int YearBuilt { get; set; }
        public string HomeType { get; set; }
        public decimal HomeValue { get; set; }
        public bool HasSecuritySystem { get; set; }
        public bool HasFireAlarm { get; set; }
        public int NumberOfOccupants { get; set; }
        public decimal QuotedPremium { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
