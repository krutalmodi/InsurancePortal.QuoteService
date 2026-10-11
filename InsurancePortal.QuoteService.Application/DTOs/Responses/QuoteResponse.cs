namespace InsurancePortal.QuoteService.Application.DTOs.Responses
{
    /// <summary>
    /// Response DTO for a complete bundled quote
    /// </summary>
    public class QuoteResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal TotalPremium { get; set; }

        /// <summary>
        /// Individual product quotes in this bundle
        /// </summary>
        public HomeQuoteResponse HomeQuote { get; set; }
        public AutoQuoteResponse AutoQuote { get; set; }
        public HealthQuoteResponse HealthQuote { get; set; }
    }
}
