namespace InsurancePortal.QuoteService.Domain.Enums
{
    /// <summary>
    /// Actor types that generate offers
    /// Maps to QuotedByTypeLookup.Code in database
    /// </summary>
    public enum QuotedByTypeEnum
    {
        System,
        Agent,
        Associate
    }
}
