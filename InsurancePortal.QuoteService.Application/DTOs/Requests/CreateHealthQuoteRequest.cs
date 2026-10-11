namespace InsurancePortal.QuoteService.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating a health insurance quote
    /// </summary>
    public class CreateHealthQuoteRequest
    {
        public int Age { get; set; }
        public string HealthStatus { get; set; }
        public bool HasPreexistingConditions { get; set; }
        public bool Smoker { get; set; }
        public string CoverageType { get; set; }
        public int Dependents { get; set; }
    }
}
