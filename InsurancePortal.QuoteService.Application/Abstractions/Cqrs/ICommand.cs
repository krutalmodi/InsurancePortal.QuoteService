namespace InsurancePortal.QuoteService.Application.Abstractions.Cqrs
{
    /// <summary>
    /// Marker for a state-changing operation that returns a result
    /// </summary>
    public interface ICommand<out TResult>
    {
    }
}
