using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Domain.Utilities
{
    /// <summary>
    /// Utility class for converting between enums and their string representations
    /// Used when querying/seeding lookup tables
    /// </summary>
    public static class LookupEnumExtensions
    {
        /// <summary>
        /// Convert enum value to its string representation (matching database Code)
        /// </summary>
        public static string ToCode(this QuoteStatusEnum value) => value.ToString();
        public static string ToCode(this ProductTypeEnum value) => value.ToString();
        public static string ToCode(this ProductQuoteStatusEnum value) => value.ToString();
        public static string ToCode(this QuotedByTypeEnum value) => value.ToString();
        public static string ToCode(this CurrencyEnum value) => value.ToString();
        public static string ToCode(this VehicleMakeEnum value) => value.ToString();
        public static string ToCode(this VehicleColorEnum value) => value.ToString();
        public static string ToCode(this DrivingRecordTypeEnum value) => value.ToString();
        public static string ToCode(this HomeTypeEnum value) => value.ToString();
        public static string ToCode(this LocationTypeEnum value) => value.ToString();
        public static string ToCode(this HealthStatusEnum value) => value.ToString();
        public static string ToCode(this CoverageTypeEnum value) => value.ToString();
        public static string ToCode(this SmokingStatusEnum value) => value.ToString();

        /// <summary>
        /// Parse string code back to enum value
        /// </summary>
        public static QuoteStatusEnum? ParseQuoteStatus(string code)
            => Enum.TryParse<QuoteStatusEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static ProductTypeEnum? ParseProductType(string code)
            => Enum.TryParse<ProductTypeEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static ProductQuoteStatusEnum? ParseProductQuoteStatus(string code)
            => Enum.TryParse<ProductQuoteStatusEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static QuotedByTypeEnum? ParseQuotedByType(string code)
            => Enum.TryParse<QuotedByTypeEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static CurrencyEnum? ParseCurrency(string code)
            => Enum.TryParse<CurrencyEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static VehicleMakeEnum? ParseVehicleMake(string code)
            => Enum.TryParse<VehicleMakeEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static VehicleColorEnum? ParseVehicleColor(string code)
            => Enum.TryParse<VehicleColorEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static DrivingRecordTypeEnum? ParseDrivingRecordType(string code)
            => Enum.TryParse<DrivingRecordTypeEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static HomeTypeEnum? ParseHomeType(string code)
            => Enum.TryParse<HomeTypeEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static LocationTypeEnum? ParseLocationType(string code)
            => Enum.TryParse<LocationTypeEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static HealthStatusEnum? ParseHealthStatus(string code)
            => Enum.TryParse<HealthStatusEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static CoverageTypeEnum? ParseCoverageType(string code)
            => Enum.TryParse<CoverageTypeEnum>(code, ignoreCase: true, out var result) ? result : null;

        public static SmokingStatusEnum? ParseSmokingStatus(string code)
            => Enum.TryParse<SmokingStatusEnum>(code, ignoreCase: true, out var result) ? result : null;
    }
}
