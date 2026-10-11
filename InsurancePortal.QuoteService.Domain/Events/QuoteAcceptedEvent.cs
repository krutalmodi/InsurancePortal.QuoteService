using InsurancePortal.QuoteService.Domain.Enums;

namespace InsurancePortal.QuoteService.Domain.Events
{
    /// <summary>
    /// Event published when a quote is accepted (converted to policy)
    /// Subscribers: Policy Service, Billing Service, Notification Service
    /// </summary>
    public class QuoteAcceptedEvent : DomainEvent
    {
        public Guid QuoteId { get; set; }
        public Guid UserId { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal TotalPremium { get; set; }
        public IEnumerable<ProductType> Products { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
