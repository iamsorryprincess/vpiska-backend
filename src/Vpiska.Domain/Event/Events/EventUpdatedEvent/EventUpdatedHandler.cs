using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.EventUpdatedEvent
{
    internal sealed class EventUpdatedHandler : IEventHandler<EventUpdatedEvent>
    {
        private readonly IUserConnectionsStorage _storage;
        private readonly IUserSender _sender;
        private readonly IEventRepository _repository;
        private readonly IEventState _eventState;

        public EventUpdatedHandler(IUserConnectionsStorage storage,
            IUserSender sender,
            IEventRepository repository,
            IEventState eventState)
        {
            _storage = storage;
            _sender = sender;
            _repository = repository;
            _eventState = eventState;
        }

        public async Task Handle(EventUpdatedEvent domainEvent)
        {
            var connections = _storage.GetConnectionsByRange(domainEvent.Coordinates.X, domainEvent.Coordinates.Y);

            if (connections.Any())
            {
                await _sender.SendEventUpdatedInfo(connections,
                    new EventUpdatedInfo()
                    {
                        EventId = domainEvent.EventId,
                        Address = domainEvent.Address,
                        UsersCount = domainEvent.UsersCount,
                        Coordinates = domainEvent.Coordinates
                    });
            }

            var model = await _eventState.GetEvent(_repository, domainEvent.EventId);

            if (model == null)
            {
                return;
            }
            
            await _eventState.UpdateLocation(domainEvent.EventId, domainEvent.Address, domainEvent.Coordinates);
        }
    }
}