using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.MediaAddedEvent
{
    internal sealed class MediaAddedHandler : IEventHandler<MediaAddedEvent>
    {
        private readonly IEventConnectionsStorage _storage;
        private readonly IEventSender _eventSender;
        private readonly IEventRepository _repository;
        private readonly IEventState _eventState;

        public MediaAddedHandler(
            IEventConnectionsStorage storage,
            IEventSender eventSender,
            IEventRepository repository,
            IEventState eventState)
        {
            _storage = storage;
            _eventSender = eventSender;
            _repository = repository;
            _eventState = eventState;
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

            var model = await _eventState.GetEvent(_repository, domainEvent.EventId);

            if (model == null)
            {
                return;
            }
            
            await _eventState.AddMediaLink(domainEvent.EventId, domainEvent.MediaId);
        }
    }
}