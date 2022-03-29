using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.EventCreatedEvent
{
    internal sealed class EventCreatedHandler : IEventHandler<EventCreatedEvent>
    {
        private readonly IUserConnectionsStorage _usersStorage;
        private readonly IUserSender _sender;
        private readonly IEventStorage _eventStorage;

        public EventCreatedHandler(IUserConnectionsStorage usersStorage,
            IUserSender sender,
            IEventStorage eventStorage)
        {
            _usersStorage = usersStorage;
            _sender = sender;
            _eventStorage = eventStorage;
        }

        public async Task Handle(EventCreatedEvent domainEvent)
        {
            var connections = _usersStorage.GetConnectionsByRange(domainEvent.Coordinates.X, domainEvent.Coordinates.Y);

            if (connections.Any())
            { 
                await _sender.SendEventCreated(connections, domainEvent.ToShortModel());
            }

            await _eventStorage.SetData(domainEvent.ToModel());
        }
    }
}