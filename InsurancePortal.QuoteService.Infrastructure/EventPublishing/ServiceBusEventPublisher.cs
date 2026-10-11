using System.Text.Json;
using Azure.Messaging.ServiceBus;
using InsurancePortal.QuoteService.Application.EventPublishing;
using InsurancePortal.QuoteService.Domain.Events;
using Microsoft.Extensions.Logging;

namespace InsurancePortal.QuoteService.Infrastructure.EventPublishing
{
    /// <summary>
    /// Implementation of event publisher using Azure Service Bus
    /// </summary>
    public class ServiceBusEventPublisher : IEventPublisher
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<ServiceBusEventPublisher> _logger;
        private const string TopicName = "insurance-events";

        public ServiceBusEventPublisher(string connectionString, ILogger<ServiceBusEventPublisher> logger)
        {
            _serviceBusClient = new ServiceBusClient(connectionString);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publish a single domain event to Service Bus
        /// </summary>
        public async Task PublishAsync(DomainEvent domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            try
            {
                var sender = _serviceBusClient.CreateSender(TopicName);
                await using (sender)
                {
                    var eventJson = JsonSerializer.Serialize(domainEvent);
                    var message = new ServiceBusMessage(eventJson)
                    {
                        Subject = domainEvent.EventType,
                        ContentType = "application/json"
                    };

                    // Add custom properties for filtering
                    message.ApplicationProperties.Add("EventType", domainEvent.EventType);
                    message.ApplicationProperties.Add("EventId", domainEvent.EventId.ToString());
                    message.ApplicationProperties.Add("Timestamp", domainEvent.Timestamp.ToUniversalTime());

                    await sender.SendMessageAsync(message);

                    _logger.LogInformation(
                        "Event published: {EventType} (EventId: {EventId})",
                        domainEvent.EventType,
                        domainEvent.EventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish event: {EventType} (EventId: {EventId})",
                    domainEvent.EventType,
                    domainEvent.EventId);
                throw;
            }
        }

        /// <summary>
        /// Publish multiple events in a batch
        /// </summary>
        public async Task PublishBatchAsync(IEnumerable<DomainEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            var eventList = events.ToList();
            if (!eventList.Any())
                return;

            try
            {
                var sender = _serviceBusClient.CreateSender(TopicName);
                await using (sender)
                {
                    var messages = new List<ServiceBusMessage>();

                    foreach (var domainEvent in eventList)
                    {
                        var eventJson = JsonSerializer.Serialize(domainEvent);
                        var message = new ServiceBusMessage(eventJson)
                        {
                            Subject = domainEvent.EventType,
                            ContentType = "application/json"
                        };

                        message.ApplicationProperties.Add("EventType", domainEvent.EventType);
                        message.ApplicationProperties.Add("EventId", domainEvent.EventId.ToString());
                        message.ApplicationProperties.Add("Timestamp", domainEvent.Timestamp.ToUniversalTime());

                        messages.Add(message);
                    }

                    await sender.SendMessagesAsync(messages);

                    _logger.LogInformation(
                        "Batch of {Count} events published",
                        eventList.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish batch of {Count} events", eventList.Count);
                throw;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync();
            }
        }
    }
}
