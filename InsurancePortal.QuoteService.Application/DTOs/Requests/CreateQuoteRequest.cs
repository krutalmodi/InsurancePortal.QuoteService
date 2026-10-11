namespace InsurancePortal.QuoteService.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating a bundled quote (can contain multiple products)
    /// </summary>
    public class CreateQuoteRequest
    {
        /// <summary>
        /// Home insurance quote details (optional)
        /// </summary>
        public CreateHomeQuoteRequest HomeQuote { get; set; }

        /// <summary>
        /// Auto insurance quote details (optional)
        /// </summary>
        public CreateAutoQuoteRequest AutoQuote { get; set; }

        /// <summary>
        /// Health insurance quote details (optional)
        /// </summary>
        public CreateHealthQuoteRequest HealthQuote { get; set; }
    }
}
