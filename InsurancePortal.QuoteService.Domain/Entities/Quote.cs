using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Domain.Entities
{
    /// <summary>
    /// Main Quote Aggregate Root
    /// Represents a bundled quote that can contain multiple product quotes
    /// Can be for a single product or multiple products (Home + Auto bundle)
    /// </summary>
    public class Quote
    {
        public Quote()
        {
            ProductQuotes = new List<ProductQuote>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            StatusId = (int)QuoteStatusEnum.Draft;
            ValidUntil = DateTime.UtcNow.AddDays(30); // Quote valid for 30 days
        }

        public Quote(Guid userId) : this()
        {
            UserId = userId;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// FK to user in IdentityMgmtDb
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Unique public reference identifier
        /// Format: QT-YYYYMMDD-XXXXX
        /// </summary>
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// FK to QuoteStatusLookup
        /// Represents overall quote lifecycle status
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// When quote expires (default 30 days from creation)
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// Sum of all ProductQuote premiums in primary currency
        /// This is a snapshot, not independently sourced
        /// </summary>
        public decimal TotalPremium { get; set; }

        /// <summary>
        /// Audit: When created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Audit: Who/what created this record
        /// Typically system or agent identifier
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

        // Navigation collection for product quotes
        public virtual ICollection<ProductQuote> ProductQuotes { get; set; }

        /// <summary>
        /// Add a product quote to this bundled quote
        /// </summary>
        public void AddProductQuote(ProductQuote productQuote, decimal? calculatedPremium = null)
        {
            if (productQuote == null)
                throw new ArgumentNullException(nameof(productQuote));

            if (ProductQuotes.Any(pq => pq.ProductType == productQuote.ProductType))
                throw new InvalidOperationException($"A {productQuote.ProductType} quote already exists in this bundle");

            productQuote.Validate();
            if (calculatedPremium < 0)
                throw new ArgumentOutOfRangeException(nameof(calculatedPremium));
            productQuote.QuoteId = Id;
            productQuote.QuotedPremium = calculatedPremium ?? productQuote.CalculatePremium();
            productQuote.ValidUntil = ValidUntil;
            ProductQuotes.Add(productQuote);

            CalculateTotalPremium();
        }

        /// <summary>
        /// Remove a product quote from this bundle
        /// </summary>
        public void RemoveProductQuote(ProductType productType)
        {
            var productQuote = ProductQuotes.FirstOrDefault(pq => pq.ProductType == productType);
            if (productQuote != null)
            {
                ProductQuotes.Remove(productQuote);
                CalculateTotalPremium();
            }
        }

        /// <summary>
        /// Calculate total premium for all product quotes in this bundle
        /// </summary>
        private void CalculateTotalPremium()
        {
            TotalPremium = ProductQuotes.Sum(pq => pq.QuotedPremium);
        }

        /// <summary>
        /// Check if quote is still valid
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ValidUntil;

        /// <summary>
        /// Mark quote as accepted
        /// </summary>
        public void MarkAsAccepted()
        {
            StatusId = (int)QuoteStatusEnum.Accepted;
        }

        /// <summary>
        /// Mark quote as converted to policy
        /// </summary>
        public void MarkAsConverted()
        {
            StatusId = (int)QuoteStatusEnum.Converted;
        }

        /// <summary>
        /// Mark quote as expired
        /// </summary>
        public void MarkAsExpired()
        {
            StatusId = (int)QuoteStatusEnum.Expired;
        }

        /// <summary>
        /// Validate the entire quote before conversion to policy
        /// </summary>
        public void Validate()
        {
            if (ProductQuotes == null || !ProductQuotes.Any())
                throw new InvalidOperationException("Quote must contain at least one product quote");

            foreach (var productQuote in ProductQuotes)
            {
                productQuote.Validate();
            }

            if (IsExpired)
                throw new InvalidOperationException("Quote has expired");
        }

        /// <summary>
        /// Get a summary of all products in this quote
        /// </summary>
        public IEnumerable<ProductType> GetProductTypes()
        {
            return ProductQuotes.Select(pq => pq.ProductType).Distinct();
        }
    }
}
