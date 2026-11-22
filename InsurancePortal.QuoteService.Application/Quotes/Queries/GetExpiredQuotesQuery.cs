using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Queries
{
    /// <summary>
    /// Query to retrieve all expired quotes
    /// </summary>
    public record GetExpiredQuotesQuery() : IQuery<IEnumerable<Quote>>;

    /// <summary>
    /// Retrieves all expired quotes
    /// </summary>
    public class GetExpiredQuotesQueryHandler : IQueryHandler<GetExpiredQuotesQuery, IEnumerable<Quote>>
    {
        private readonly IQuoteRepository _repository;

        public GetExpiredQuotesQueryHandler(IQuoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<Quote>> HandleAsync(GetExpiredQuotesQuery query)
        {
            return await _repository.GetExpiredQuotesAsync();
        }
    }
}
