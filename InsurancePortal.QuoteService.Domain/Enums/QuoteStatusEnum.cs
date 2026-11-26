namespace InsurancePortal.QuoteService.Domain.Enums
{
    /// <summary>
    /// Quote lifecycle statuses, keyed by QuoteStatusLookup.Id.
    /// </summary>
    public enum QuoteStatusEnum
    {
        Draft = 1,
        Submitted = 2,
        InProgress = 3,
        Quoted = 4,
        Expired = 5,
        Accepted = 6,
        Converted = 7,
        Declined = 8
    }
}
