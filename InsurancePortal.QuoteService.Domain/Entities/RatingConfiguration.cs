namespace InsurancePortal.QuoteService.Domain.Entities
{
    public class RatingPlan
    {
        public int Id { get; set; }
        public int ProductTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public int ProvinceId { get; set; }
        public int CurrencyId { get; set; }
        public int RiskDatasetVersion { get; set; }
        public DateTime EffectiveFromUtc { get; set; }
        public DateTime? EffectiveToUtc { get; set; }
        public decimal BaseRate { get; set; }
        public bool IsPercentageOfHomeValue { get; set; }
        public bool Published { get; set; }
    }

    public class RatingFactor
    {
        public int Id { get; set; }
        public int RatingPlanId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public decimal Multiplier { get; set; }
    }

    public class PostalRisk
    {
        public int Id { get; set; }
        public int DatasetVersion { get; set; }
        public int CountryId { get; set; }
        public int ProvinceId { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public int Hail { get; set; }
        public int Theft { get; set; }
        public int Fire { get; set; }
        public int Crash { get; set; }
    }
}
