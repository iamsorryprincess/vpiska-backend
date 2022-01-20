using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.MediaRemovedEvent
{
    public sealed class MediaRemovedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public string MediaId { get; set; }
    }
}