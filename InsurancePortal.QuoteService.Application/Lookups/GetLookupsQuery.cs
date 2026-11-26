using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;

namespace InsurancePortal.QuoteService.Application.Lookups
{
    public record GetLookupsQuery(LookupType Type, bool IncludeInactive, int? VehicleMakeId) : IQuery<IReadOnlyList<LookupItem>>;

    public class GetLookupsQueryHandler : IQueryHandler<GetLookupsQuery, IReadOnlyList<LookupItem>>
    {
        private readonly ILookupRepository _repository;

        public GetLookupsQueryHandler(ILookupRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<IReadOnlyList<LookupItem>> HandleAsync(GetLookupsQuery query)
        {
            if (!Enum.IsDefined(query.Type))
                throw new ArgumentOutOfRangeException(nameof(query), "Unknown lookup type.");
            if (query.VehicleMakeId.HasValue && (query.Type != LookupType.VehicleModels || query.VehicleMakeId <= 0))
                throw new ArgumentException("A positive vehicleMakeId can only be used with vehicle models.", nameof(query));

            return _repository.GetAsync(query.Type, query.IncludeInactive, query.VehicleMakeId);
        }
    }
}
