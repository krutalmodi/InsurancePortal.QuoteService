namespace InsurancePortal.QuoteService.Domain.Entities.Lookups
{
    /// <summary>
    /// Lookup for vehicle models
    /// Related to VehicleMakeLookup via VehicleMakeId foreign key
    /// </summary>
    public class VehicleModelLookup : BaseLookup
    {
        /// <summary>
        /// Foreign key to VehicleMakeLookup
        /// Ensures model belongs to correct make
        /// </summary>
        public int VehicleMakeId { get; set; }

        /// <summary>
        /// Navigation property
        /// </summary>
        public virtual VehicleMakeLookup? Make { get; set; }
    }
}
