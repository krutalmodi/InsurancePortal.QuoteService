using InsurancePortal.QuoteService.Application.Abstractions.Cqrs;
using InsurancePortal.QuoteService.Application.Repositories;
using InsurancePortal.QuoteService.Domain.Entities;

namespace InsurancePortal.QuoteService.Application.Quotes.Queries
{
    /// <summary>
    /// Query to retrieve a quote by ID
    /// </summary>
    public record GetQuoteByIdQuery(Guid QuoteId) : IQuery<Quote?>;

    /// <summary>
    /// Retrieves a single quote by ID
    /// </summary>
    public class GetQuoteByIdQueryHandler : IQueryHandler<GetQuoteByIdQuery, Quote?>
    {
        private readonly IQuoteRepository _repository;

        public GetQuoteByIdQueryHandler(IQuoteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Quote?> HandleAsync(GetQuoteByIdQuery query)
        {
            if (query.QuoteId == Guid.Empty)
                throw new ArgumentException("Quote ID cannot be empty", nameof(query));

            return await _repository.GetByIdAsync(query.QuoteId);
        }
    }
}
