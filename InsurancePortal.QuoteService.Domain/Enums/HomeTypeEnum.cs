namespace InsurancePortal.QuoteService.Domain.Enums
{
    /// <summary>
    /// Residential property types
    /// Maps to HomeTypeLookup.Code in database
    /// </summary>
    public enum HomeTypeEnum
    {
        Detached,
        SemiDetached,
        Townhouse,
        Condominium,
        MobileHome,
        Other
    }
}
