using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.ValueObjects;

namespace InsurancePortal.QuoteService.Domain.Entities
{
    /// <summary>
    /// Auto Insurance Quote - inherits from ProductQuote
    /// </summary>
    public class AutoQuote : ProductQuote
    {
        public AutoQuote() : base()
        {
            Discriminator = nameof(AutoQuote);
        }

        public AutoQuote(AutoQuoteDetails details) : base()
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));
            Details = details;
            Discriminator = nameof(AutoQuote);
        }

        public AutoQuoteDetails? Details { get; set; }

        public override ProductType ProductType => ProductType.Auto;

        public override void Validate()
        {
            if (Details == null)
                throw new InvalidOperationException("Auto quote details are required");

            if (string.IsNullOrWhiteSpace(Details.VIN))
                throw new InvalidOperationException("VIN is required");

            if (Details.NumberOfDrivers <= 0)
                throw new InvalidOperationException("At least one driver is required");
        }

        public override decimal CalculatePremium()
        {
            Validate();

            // Base premium calculation (simplified for demo)
            // In production, this would call an AI/ML service for risk assessment
            decimal basePremium = 1000m; // Base premium

            // Adjustment based on vehicle age
            int vehicleAge = DateTime.UtcNow.Year - Details.Year;
            if (vehicleAge > 10)
                basePremium *= 0.95m;
            else if (vehicleAge < 3)
                basePremium *= 1.10m;

            // Adjustment based on mileage
            if (Details.Mileage > 100000)
                basePremium *= 1.05m;

            // Adjustment based on number of drivers
            basePremium += (Details.NumberOfDrivers - 1) * 150m;

            // Adjustment based on estimated annual mileage
            if (Details.EstimatedAnnualMileage > 20000)
                basePremium *= 1.08m;

            return Math.Round(basePremium, 2);
        }
    }
}
