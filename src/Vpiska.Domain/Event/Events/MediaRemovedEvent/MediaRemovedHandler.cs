using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.MediaRemovedEvent
{
    internal sealed class MediaRemovedHandler : IEventHandler<MediaRemovedEvent>
    {
        private readonly IEventConnectionsStorage _storage;
        private readonly IEventSender _eventSender;

        public MediaRemovedHandler(IEventConnectionsStorage storage, IEventSender eventSender)
        {
            _storage = storage;
            _eventSender = eventSender;
        }

        public async Task Handle(MediaRemovedEvent domainEvent)
        {
            if (_storage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _storage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await _eventSender.NotifyMediaRemoved(connections, domainEvent.MediaId);
                }
            }
        }
    }
}