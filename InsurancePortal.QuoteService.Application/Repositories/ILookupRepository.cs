using InsurancePortal.QuoteService.Application.Lookups;

namespace InsurancePortal.QuoteService.Application.Repositories
{
    public interface ILookupRepository
    {
        Task<IReadOnlyList<LookupItem>> GetAsync(LookupType type, bool includeInactive, int? vehicleMakeId);
    }
}
