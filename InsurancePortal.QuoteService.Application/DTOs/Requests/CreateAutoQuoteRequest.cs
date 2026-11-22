namespace InsurancePortal.QuoteService.Application.DTOs.Requests
{
    /// <summary>
    /// Request DTO for creating an auto insurance quote
    /// </summary>
    public class CreateAutoQuoteRequest
    {
        /// <summary>
        /// Address of the vehicle's primary location; a Location is created on the fly for the quote
        /// </summary>
        public LocationRequest Location { get; set; }

        public string VIN { get; set; }
        public int VehicleMakeId { get; set; }
        public int VehicleModelId { get; set; }
        public int Year { get; set; }
        public int VehicleColorId { get; set; }
        public int Mileage { get; set; }
        public string DrivingRecord { get; set; }
        public int NumberOfDrivers { get; set; }
        public decimal EstimatedAnnualMileage { get; set; }
    }
}
