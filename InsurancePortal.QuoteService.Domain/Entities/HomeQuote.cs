using InsurancePortal.QuoteService.Domain.Enums;
using InsurancePortal.QuoteService.Domain.ValueObjects;

namespace InsurancePortal.QuoteService.Domain.Entities
{
    /// <summary>
    /// Home Insurance Quote - inherits from ProductQuote
    /// Stores home-specific underwriting/application inputs
    /// Offer details (premium, status, provider) are in ProductQuote base class
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

        /// <summary>
        /// FK to Location (address of the property)
        /// </summary>
        public Guid? LocationId { get; set; }

        public string? UnitNumber { get; set; }

        public string? StreetAddress1 { get; set; }

        public string? StreetAddress2 { get; set; }

        public string? City { get; set; }

        public string? PostalCode { get; set; }

        public int? ProvinceId { get; set; }

        public int? CountryId { get; set; }

        /// <summary>
        /// Home-specific application details as owned value object
        /// </summary>
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
