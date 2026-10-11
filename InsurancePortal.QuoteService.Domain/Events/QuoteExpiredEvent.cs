namespace InsurancePortal.QuoteService.Domain.Events
{
    /// <summary>
    /// Event published when a quote expires
    /// Subscribers: Notification Service (cleanup)
    /// </summary>
    public class QuoteExpiredEvent : DomainEvent
    {
        public Guid QuoteId { get; set; }
        public Guid UserId { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
