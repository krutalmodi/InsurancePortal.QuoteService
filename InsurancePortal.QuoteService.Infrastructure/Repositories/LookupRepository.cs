using InsurancePortal.QuoteService.Application.Lookups;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities.Lookups;
using InsurancePortal.QuoteService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InsurancePortal.QuoteService.Infrastructure.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly QuoteDbContext _context;

        public LookupRepository(QuoteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<LookupItem>> GetAsync(LookupType type, bool includeInactive, int? vehicleMakeId)
        {
            return type switch
            {
                LookupType.QuoteStatuses => await GetSimpleAsync(_context.QuoteStatusLookups, includeInactive),
                LookupType.ProductTypes => await GetSimpleAsync(_context.ProductTypeLookups, includeInactive),
                LookupType.ProductQuoteStatuses => await GetSimpleAsync(_context.ProductQuoteStatusLookups, includeInactive),
                LookupType.QuotedByTypes => await GetSimpleAsync(_context.QuotedByTypeLookups, includeInactive),
                LookupType.VehicleColors => await GetSimpleAsync(_context.VehicleColorLookups, includeInactive),
                LookupType.DrivingRecordTypes => await GetSimpleAsync(_context.DrivingRecordTypeLookups, includeInactive),
                LookupType.HomeTypes => await GetSimpleAsync(_context.HomeTypeLookups, includeInactive),
                LookupType.HealthStatuses => await GetSimpleAsync(_context.HealthStatusLookups, includeInactive),
                LookupType.CoverageTypes => await GetSimpleAsync(_context.CoverageTypeLookups, includeInactive),
                LookupType.SmokingStatuses => await GetSimpleAsync(_context.SmokingStatusLookups, includeInactive),
                LookupType.InsuranceProviders => await GetSimpleAsync(_context.InsuranceProviderLookups, includeInactive),
                LookupType.VehicleMakes => await GetSimpleAsync(_context.VehicleMakeLookups, includeInactive),
                LookupType.VehicleModels => await GetVehicleModelsAsync(includeInactive, vehicleMakeId),
                LookupType.VehicleTypes => await GetSimpleAsync(_context.VehicleTypeLookups, includeInactive),
                LookupType.Currencies => await GetCurrenciesAsync(includeInactive),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown lookup type.")
            };
        }

        private static IQueryable<T> Ordered<T>(IQueryable<T> query, bool includeInactive) where T : BaseLookup
        {
            if (!includeInactive)
                query = query.Where(item => item.IsActive);

            return query.AsNoTracking().OrderBy(item => item.SortOrder).ThenBy(item => item.Name).ThenBy(item => item.Id);
        }

        private static async Task<IReadOnlyList<LookupItem>> GetSimpleAsync<T>(IQueryable<T> query, bool includeInactive) where T : BaseLookup
        {
            return await Ordered(query, includeInactive)
                .Select(item => new LookupItem(item.Id, item.Code, item.Name, item.Description, item.IsActive, item.SortOrder, null, null, null))
                .ToListAsync();
        }

        private async Task<IReadOnlyList<LookupItem>> GetVehicleModelsAsync(bool includeInactive, int? vehicleMakeId)
        {
            var query = _context.VehicleModelLookups.AsQueryable();
            if (vehicleMakeId.HasValue)
                query = query.Where(item => item.VehicleMakeId == vehicleMakeId.Value);

            return await Ordered(query, includeInactive)
                .Select(item => new LookupItem(item.Id, item.Code, item.Name, item.Description, item.IsActive, item.SortOrder, item.VehicleMakeId, null, null))
                .ToListAsync();
        }

        private async Task<IReadOnlyList<LookupItem>> GetCurrenciesAsync(bool includeInactive)
        {
            return await Ordered(_context.CurrencyLookups, includeInactive)
                .Select(item => new LookupItem(item.Id, item.Code, item.Name, item.Description, item.IsActive, item.SortOrder, null, item.IsoCode, item.DecimalPlaces))
                .ToListAsync();
        }
    }
}
