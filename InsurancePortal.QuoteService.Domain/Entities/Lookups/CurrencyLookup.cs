namespace InsurancePortal.QuoteService.Domain.Entities.Lookups
{
    /// <summary>
    /// Lookup for currency types (ISO 4217 codes)
    /// Use CurrencyEnum for type-safe currency values
    /// </summary>
    public class CurrencyLookup : BaseLookup
    {
        /// <summary>
        /// ISO 4217 currency code (e.g., "USD", "CAD")
        /// </summary>
        public string IsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Number of decimal places for minor units
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;
    }
}
