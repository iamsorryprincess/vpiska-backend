using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.MediaRemovedEvent
{
    internal sealed class MediaRemovedHandler : IEventHandler<MediaRemovedEvent>
    {
        private readonly IEventConnectionsStorage _eventConnectionsStorage;
        private readonly IEventSender _eventSender;
        private readonly IEventRepository _repository;
        private readonly IEventStorage _eventStorage;

        public MediaRemovedHandler(IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IEventRepository repository,
            IEventStorage eventStorage)
        {
            _eventConnectionsStorage = eventConnectionsStorage;
            _eventSender = eventSender;
            _repository = repository;
            _eventStorage = eventStorage;
        }

        public async Task Handle(MediaRemovedEvent domainEvent)
        {
            if (_eventConnectionsStorage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _eventConnectionsStorage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await _eventSender.NotifyMediaRemoved(connections, domainEvent.MediaId);
                }
            }

            var model = await _eventStorage.GetEvent(_repository, domainEvent.EventId);

            if (model == null)
            {
                return;
            }
            
            await _eventStorage.RemoveMediaLink(domainEvent.EventId, domainEvent.MediaId);
        }
    }
}