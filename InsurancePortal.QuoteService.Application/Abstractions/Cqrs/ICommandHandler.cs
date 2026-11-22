namespace InsurancePortal.QuoteService.Application.Abstractions.Cqrs
{
    /// <summary>
    /// Handles a single command type
    /// </summary>
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }
}
