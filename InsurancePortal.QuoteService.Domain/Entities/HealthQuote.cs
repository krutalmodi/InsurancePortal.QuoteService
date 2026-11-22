using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.ValueObjects;

namespace InsurancePortal.QuoteService.Domain.Entities
{
    /// <summary>
    /// Health Insurance Quote - inherits from ProductQuote
    /// </summary>
    public class HealthQuote : ProductQuote
    {
        public HealthQuote() : base()
        {
            Discriminator = nameof(HealthQuote);
        }

        public HealthQuote(HealthQuoteDetails details) : base()
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));
            Details = details;
            Discriminator = nameof(HealthQuote);
        }

        /// <summary>
        /// FK to Location (address of the applicant)
        /// </summary>
        public Guid? LocationId { get; set; }

        public string? UnitNumber { get; set; }

        public string? StreetAddress1 { get; set; }

        public string? StreetAddress2 { get; set; }

        public string? City { get; set; }

        public string? PostalCode { get; set; }

        public int? ProvinceId { get; set; }

        public int? CountryId { get; set; }

        public HealthQuoteDetails? Details { get; set; }

        public override ProductType ProductType => ProductType.Health;

        public override void Validate()
        {
            if (Details == null)
                throw new InvalidOperationException("Health quote details are required");

            if (Details.Age < 18 || Details.Age > 125)
                throw new InvalidOperationException("Invalid age for health insurance");
        }

        public override decimal CalculatePremium()
        {
            Validate();

            // Base premium calculation (simplified for demo)
            // In production, this would call an AI/ML service for risk assessment
            decimal basePremium = 500m; // Base premium

            // Age-based adjustment
            if (Details.Age < 30)
                basePremium *= 0.80m;
            else if (Details.Age >= 50)
                basePremium *= 1.50m;
            else if (Details.Age >= 40)
                basePremium *= 1.25m;

            // Pre-existing conditions adjustment
            if (Details.HasPreexistingConditions)
                basePremium *= 1.35m;

            // Smoker adjustment
            if (Details.Smoker)
                basePremium *= 1.50m;

            // Dependent adjustment
            basePremium += Details.Dependents * 100m;

            return Math.Round(basePremium, 2);
        }
    }
}
