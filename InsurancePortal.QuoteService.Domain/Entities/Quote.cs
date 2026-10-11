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
            Status = QuoteStatus.Active;
            ValidUntil = DateTime.UtcNow.AddDays(30); // Quote valid for 30 days
        }

        public Quote(Guid userId) : this()
        {
            UserId = userId;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public QuoteStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal TotalPremium { get; set; }
        public string? ReferenceNumber { get; set; }

        // Navigation collection for product quotes
        public virtual ICollection<ProductQuote> ProductQuotes { get; set; }

        /// <summary>
        /// Add a product quote to this bundled quote
        /// </summary>
        public void AddProductQuote(ProductQuote productQuote)
        {
            if (productQuote == null)
                throw new ArgumentNullException(nameof(productQuote));

            if (ProductQuotes.Any(pq => pq.ProductType == productQuote.ProductType))
                throw new InvalidOperationException($"A {productQuote.ProductType} quote already exists in this bundle");

            productQuote.QuoteId = Id;
            productQuote.QuotedPremium = productQuote.CalculatePremium();
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
        /// Mark quote as converted to policy
        /// </summary>
        public void MarkAsConverted()
        {
            Status = QuoteStatus.Converted;
        }

        /// <summary>
        /// Mark quote as expired
        /// </summary>
        public void MarkAsExpired()
        {
            Status = QuoteStatus.Expired;
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
