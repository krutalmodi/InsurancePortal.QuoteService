using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Domain.Events
{
    /// <summary>
    /// Event published when a new quote is created
    /// Subscribers: Notification Service
    /// </summary>
    public class QuoteCreatedEvent : DomainEvent
    {
        public Guid QuoteId { get; set; }
        public Guid UserId { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal TotalPremium { get; set; }
        public IEnumerable<ProductType> Products { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
