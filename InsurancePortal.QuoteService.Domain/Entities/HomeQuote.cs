using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.ValueObjects;

namespace InsurancePortal.QuoteService.Domain.Entities
{
    /// <summary>
    /// Home Insurance Quote - inherits from ProductQuote
    /// </summary>
    public class HomeQuote : ProductQuote
    {
        public HomeQuote() : base()
        {
            Discriminator = nameof(HomeQuote);
        }

        public HomeQuote(HomeQuoteDetails details) : base()
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));
            Details = details;
            Discriminator = nameof(HomeQuote);
        }

        public HomeQuoteDetails? Details { get; set; }

        public override ProductType ProductType => ProductType.Home;

        public override void Validate()
        {
            if (Details == null)
                throw new InvalidOperationException("Home quote details are required");

            if (Details.SquareFootage <= 0)
                throw new InvalidOperationException("Square footage must be greater than 0");

            if (Details.HomeValue <= 0)
                throw new InvalidOperationException("Home value must be greater than 0");
        }

        public override decimal CalculatePremium()
        {
            Validate();

            // Base premium calculation (simplified for demo)
            // In production, this would call an AI/ML service
            decimal basePremium = Details.HomeValue * 0.005m; // 0.5% of home value

            // Discount for security features
            if (Details.HasSecuritySystem)
                basePremium *= 0.95m;
            if (Details.HasFireAlarm)
                basePremium *= 0.97m;

            // Adjustment based on age
            int homeAge = DateTime.UtcNow.Year - Details.YearBuilt;
            if (homeAge > 50)
                basePremium *= 1.10m;

            return Math.Round(basePremium, 2);
        }
    }
}
