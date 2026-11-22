namespace InsurancePortal.QuoteService.Domain.Entities.Lookups
{
    /// <summary>
    /// Lookup for insurance providers/companies
    /// Seeded with 'Allstate Insurance' as default
    /// </summary>
    public class InsuranceProviderLookup : BaseLookup
    {
        /// <summary>
        /// Company registration/identifier
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Contact email for quotes/inquiries
        /// </summary>
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
}
