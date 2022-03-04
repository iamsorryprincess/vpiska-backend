using Vpiska.Domain.Event.Events.EventClosedEvent;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Commands.CloseEventCommand
{
    public sealed class CloseEventCommand
    {
        public string EventId { get; set; }

        public string OwnerId { get; set; }

        public EventClosedEvent ToEvent(Coordinates coordinates) => new()
        {
            EventId = EventId,
            Coordinates = coordinates
        };
    }
}