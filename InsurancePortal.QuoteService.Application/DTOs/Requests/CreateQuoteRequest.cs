using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating a single product quote
    /// Each request creates or adds to a quote with exactly one product type
    /// </summary>
    public class CreateQuoteRequest
    {
        /// <summary>
        /// The type of product being quoted (Home, Auto, or Health)
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// Home insurance quote details (required if ProductType is Home)
        /// </summary>
        public CreateHomeQuoteRequest? HomeQuote { get; set; }

        /// <summary>
        /// Auto insurance quote details (required if ProductType is Auto)
        /// </summary>
        public CreateAutoQuoteRequest? AutoQuote { get; set; }

        /// <summary>
        /// Health insurance quote details (required if ProductType is Health)
        /// </summary>
        public CreateHealthQuoteRequest? HealthQuote { get; set; }
    }
}
