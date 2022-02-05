using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.EventUpdatedEvent
{
    internal sealed class EventUpdatedHandler : IEventHandler<EventUpdatedEvent>
    {
        private readonly IUserConnectionsStorage _storage;
        private readonly IUserSender _sender;

        public EventUpdatedHandler(IUserConnectionsStorage storage, IUserSender sender)
        {
            _storage = storage;
            _sender = sender;
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
        }
    }
}