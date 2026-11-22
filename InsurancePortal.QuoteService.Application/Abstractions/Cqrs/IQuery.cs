namespace InsurancePortal.QuoteService.Application.Abstractions.Cqrs
{
    /// <summary>
    /// Marker for a read-only operation that returns a result
    /// </summary>
    public interface IQuery<out TResult>
    {
    }
}
