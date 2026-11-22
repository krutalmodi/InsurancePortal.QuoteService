namespace InsurancePortal.QuoteService.Domain.Enums
{
    /// <summary>
    /// Health status classifications
    /// Maps to HealthStatusLookup.Code in database
    /// </summary>
    public enum HealthStatusEnum
    {
        None,
        PreexistingDiabetes,
        PreexistingHypertension,
        PreexistingHeartDisease,
        PreexistingCancer,
        OtherPreexistingCondition,
        MultiplePreexistingConditions
    }
}
