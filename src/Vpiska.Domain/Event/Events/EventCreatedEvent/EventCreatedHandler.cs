using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.EventCreatedEvent
{
    internal sealed class EventCreatedHandler : IEventHandler<EventCreatedEvent>
    {
        private readonly IUserConnectionsStorage _storage;
        private readonly IUserSender _sender;

        public EventCreatedHandler(IUserConnectionsStorage storage, IUserSender sender)
        {
            _storage = storage;
            _sender = sender;
        }

        public async Task Handle(EventCreatedEvent domainEvent)
        {
            var connections = _storage.GetConnectionsByRange(domainEvent.Coordinates.X, domainEvent.Coordinates.Y);

            if (connections.Any())
            { 
                await _sender.SendEventCreated(connections, domainEvent.ToShortResponse());
            }
        }
    }
}