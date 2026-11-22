using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Domain.Entities
{
    /// <summary>
    /// Abstract base class for all product-specific quote offers
    /// Uses Table-Per-Type (TPT) inheritance pattern
    /// Stores offer metadata: premium, provider, status, currency, validity
    /// Does NOT store product-specific underwriting inputs (see AutoQuote, HomeQuote, HealthQuote)
    /// </summary>
    public abstract class ProductQuote
    {
        protected ProductQuote()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key to Quote aggregate root
        /// </summary>
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Abstract property: type of product (Auto, Home, Health)
        /// </summary>
        public abstract ProductType ProductType { get; }

        /// <summary>
        /// FK to ProductTypeLookup
        /// Redundant with ProductType but used for queries and reporting
        /// </summary>
        public int ProductTypeId { get; set; }

        /// <summary>
        /// FK to ProductQuoteStatusLookup
        /// Tracks offer lifecycle: Pending, Offered, Accepted, Rejected, Expired, Withdrawn
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// The quoted premium amount for this offer
        /// </summary>
        public decimal QuotedPremium { get; set; }

        /// <summary>
        /// FK to CurrencyLookup
        /// Identifies the currency for this offer
        /// Default: USD
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// When this offer expires and can no longer be accepted
        /// For Health products, default is 30 days from issuance
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// FK to InsuranceProviderLookup
        /// Identifies which provider issued this offer
        /// Can be null for system-generated quotes
        /// </summary>
        public int? ProviderId { get; set; }

        /// <summary>
        /// ID of the user/agent/system actor who created this offer
        /// Can be null for system-generated offers
        /// </summary>
        public Guid? QuotedBy { get; set; }

        /// <summary>
        /// FK to QuotedByTypeLookup
        /// Categorizes who created the offer: System, Agent, Associate
        /// </summary>
        public int? QuotedByTypeId { get; set; }

        /// <summary>
        /// Audit: When created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Audit: Who/what created this record
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Audit: When last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Audit: Who/what last updated this record
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Discriminator property for EF Core TPT pattern
        /// </summary>
        public string? Discriminator { get; set; }

        public abstract void Validate();
        public abstract decimal CalculatePremium();
    }
}
