namespace InsurancePortal.QuoteService.Domain.Events
{
    /// <summary>
    /// Base event class for all domain events
    /// </summary>
    public abstract class DomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string EventType => GetType().Name;
    }
}
