using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.EventClosedEvent
{
    internal sealed class EventClosedHandler : IEventHandler<EventClosedEvent>
    {
        private readonly ILogger<EventClosedHandler> _logger;
        private readonly IConnectionsStorage _storage;
        private readonly IEventSender _eventSender;
        
        public EventClosedHandler(ILogger<EventClosedHandler> logger,
            IConnectionsStorage storage,
            IEventSender eventSender)
        {
            _logger = logger;
            _storage = storage;
            _eventSender = eventSender;
        }
        
        public async Task Handle(EventClosedEvent domainEvent)
        {
            if (_storage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _storage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await Task.WhenAll(connections.Select(_eventSender.SendCloseStatus));
                }

                if (!_storage.RemoveEventGroup(domainEvent.EventId))
                {
                    _logger.LogWarning("Can't remove event group {}", domainEvent.EventId);
                }
            }
        }
    }
}