using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Queries
{
    /// <summary>
    /// Query to retrieve all quotes for a user
    /// </summary>
    public record GetUserQuotesQuery(Guid UserId) : IQuery<IEnumerable<Quote>>;

    /// <summary>
    /// Retrieves all quotes belonging to a user
    /// </summary>
    public class GetUserQuotesQueryHandler : IQueryHandler<GetUserQuotesQuery, IEnumerable<Quote>>
    {
        private readonly IQuoteRepository _repository;

        public GetUserQuotesQueryHandler(IQuoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<Quote>> HandleAsync(GetUserQuotesQuery query)
        {
            if (query.UserId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(query));

            return await _repository.GetByUserIdAsync(query.UserId);
        }
    }
}
