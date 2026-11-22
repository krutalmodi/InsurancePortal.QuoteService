namespace InsurancePortal.QuoteService.Domain.Enums
{
    /// <summary>
    /// Individual ProductQuote offer statuses
    /// Maps to ProductQuoteStatusLookup.Code in database
    /// </summary>
    public enum ProductQuoteStatusEnum
    {
        Pending = 1,
        Offered,
        Accepted,
        Rejected,
        Expired,
        Withdrawn
    }
}
