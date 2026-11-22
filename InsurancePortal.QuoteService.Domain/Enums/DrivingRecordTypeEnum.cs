namespace InsurancePortal.QuoteService.Domain.Enums
{
    /// <summary>
    /// Driving record classifications
    /// Maps to DrivingRecordTypeLookup.Code in database
    /// </summary>
    public enum DrivingRecordTypeEnum
    {
        Clean,
        MinorViolation,
        MajorViolation,
        Accident,
        MultipleViolations,
        MultipleAccidents,
        Other
    }
}
