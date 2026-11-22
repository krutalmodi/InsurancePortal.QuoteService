namespace InsurancePortal.QuoteService.Application.Abstractions.Cqrs
{
    /// <summary>
    /// Routes commands and queries to their registered handlers
    /// </summary>
    public interface IDispatcher
    {
        Task<TResult> SendAsync<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>;

        Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>;
    }
}
