using System.Collections.Generic;

namespace InsurancePortal.QuoteService.Domain.Entities.Lookups
{
    /// <summary>
    /// Lookup for vehicle manufacturers
    /// Use VehicleMakeEnum for type-safe make values
    /// </summary>
    public class VehicleMakeLookup : BaseLookup
    {
        /// <summary>
        /// Collection of vehicle models for this make
        /// </summary>
        public ICollection<VehicleModelLookup> Models { get; set; } = new HashSet<VehicleModelLookup>();
    }
}
