using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Domain.Entities
{
    /// <summary>
    /// Abstract base class for all product-specific quotes
    /// Uses Table-Per-Hierarchy (TPH) pattern
    /// </summary>
    public abstract class ProductQuote
    {
        protected ProductQuote()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public Guid QuoteId { get; set; }
        public abstract ProductType ProductType { get; }
        public decimal QuotedPremium { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Discriminator property for EF Core TPH pattern
        /// </summary>
        public string? Discriminator { get; set; }

        public abstract void Validate();
        public abstract decimal CalculatePremium();
    }
}
