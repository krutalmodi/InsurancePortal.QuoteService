namespace InsurancePortal.QuoteService.Application.Abstractions.Cqrs
{
    /// <summary>
    /// Handles a single query type
    /// </summary>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
