using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.MediaAddedEvent
{
    internal sealed class MediaAddedHandler : IEventHandler<MediaAddedEvent>
    {
        private readonly IConnectionsStorage _storage;
        private readonly IEventSender _eventSender;

        public MediaAddedHandler(IConnectionsStorage storage, IEventSender eventSender)
        {
            _storage = storage;
            _eventSender = eventSender;
        }

        public async Task Handle(MediaAddedEvent domainEvent)
        {
            if (_storage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _storage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await _eventSender.NotifyMediaAdded(connections, domainEvent.MediaId);
                }
            }
        }
    }
}