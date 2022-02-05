using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.EventClosedEvent
{
    public sealed class EventClosedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public Coordinates Coordinates { get; set; }
    }
}