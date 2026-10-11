namespace InsurancePortal.QuoteService.Application.DTOs.Responses
{
    /// <summary>
    /// Response DTO for health insurance quote
    /// </summary>
    public class HealthQuoteResponse
    {
        public Guid Id { get; set; }
        public int Age { get; set; }
        public string HealthStatus { get; set; }
        public bool HasPreexistingConditions { get; set; }
        public bool Smoker { get; set; }
        public string CoverageType { get; set; }
        public int Dependents { get; set; }
        public decimal QuotedPremium { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
