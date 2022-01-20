using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.MediaAddedEvent
{
    public sealed class MediaAddedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public string MediaId { get; set; }
    }
}