using InsurancePortal.QuoteService.Domain.Events;

namespace InsurancePortal.QuoteService.Application.EventPublishing
{
    /// <summary>
    /// Interface for publishing domain events to message broker
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish a domain event to the message broker
        /// </summary>
        /// <param name="domainEvent">The event to publish</param>
        /// <returns>Awaitable task</returns>
        Task PublishAsync(DomainEvent domainEvent);

        /// <summary>
        /// Publish multiple events in batch
        /// </summary>
        /// <param name="events">Collection of events to publish</param>
        /// <returns>Awaitable task</returns>
        Task PublishBatchAsync(IEnumerable<DomainEvent> events);
    }
}
