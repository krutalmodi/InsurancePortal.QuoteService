namespace InsurancePortal.QuoteService.Domain.Enums
{
    /// <summary>
    /// Quote lifecycle statuses
    /// Maps to QuoteStatusLookup.Code in database
    /// </summary>
    public enum QuoteStatusEnum
    {
        Draft = 1,
        Submitted,
        InProgress,
        Quoted,
        Accepted,
        Rejected,
        Expired,
        Cancelled
    }
}
