namespace InsurancePortal.QuoteService.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating a health insurance quote
    /// </summary>
    public class CreateHealthQuoteRequest
    {
        /// <summary>
        /// Address of the applicant; a Location is created on the fly for the quote
        /// </summary>
        public LocationRequest Location { get; set; }

        public int Age { get; set; }
        public string HealthStatus { get; set; }
        public bool HasPreexistingConditions { get; set; }
        public bool Smoker { get; set; }
        public string CoverageType { get; set; }
        public int Dependents { get; set; }
    }
}
