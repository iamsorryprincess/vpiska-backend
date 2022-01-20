using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.EventClosedEvent
{
    public sealed class EventClosedEvent : IDomainEvent
    {
        public string EventId { get; set; }
    }
}