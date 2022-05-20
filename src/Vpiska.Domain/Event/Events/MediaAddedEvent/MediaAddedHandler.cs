using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.MediaAddedEvent
{
    internal sealed class MediaAddedHandler : IEventHandler<MediaAddedEvent>
    {
        private readonly IEventConnectionsStorage _eventConnectionsStorage;
        private readonly IEventSender _eventSender;
        private readonly IEventRepository _repository;
        private readonly IEventStorage _eventStorage;

        public MediaAddedHandler(
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IEventRepository repository,
            IEventStorage eventStorage)
        {
            _eventConnectionsStorage = eventConnectionsStorage;
            _eventSender = eventSender;
            _repository = repository;
            _eventStorage = eventStorage;
        }

        public async Task Handle(MediaAddedEvent domainEvent)
        {
            if (_eventConnectionsStorage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _eventConnectionsStorage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await _eventSender.NotifyMediaAdded(connections, domainEvent.MediaInfo.Id);
                }
            }

            var model = await _eventStorage.GetEvent(_repository, domainEvent.EventId);

            if (model == null)
            {
                return;
            }
            
            await _eventStorage.AddMediaLink(domainEvent.EventId, domainEvent.MediaInfo);
        }
    }
}