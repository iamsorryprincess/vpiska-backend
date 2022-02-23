using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.EventUpdatedEvent
{
    internal sealed class EventUpdatedHandler : IEventHandler<EventUpdatedEvent>
    {
        private readonly IUserConnectionsStorage _usersStorage;
        private readonly IUserSender _sender;
        private readonly IEventRepository _repository;
        private readonly IEventStorage _eventStorage;

        public EventUpdatedHandler(IUserConnectionsStorage usersStorage,
            IUserSender sender,
            IEventRepository repository,
            IEventStorage eventStorage)
        {
            _usersStorage = usersStorage;
            _sender = sender;
            _repository = repository;
            _eventStorage = eventStorage;
        }

        public async Task Handle(EventUpdatedEvent domainEvent)
        {
            var connections = _usersStorage.GetConnectionsByRange(domainEvent.Coordinates.X, domainEvent.Coordinates.Y);

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

            var model = await _eventStorage.GetEvent(_repository, domainEvent.EventId);

            if (model == null)
            {
                return;
            }
            
            await _eventStorage.UpdateLocation(domainEvent.EventId, domainEvent.Address, domainEvent.Coordinates);
        }
    }
}