using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.EventUpdatedEvent
{
    public sealed class EventUpdatedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public string Address { get; set; }

        public int UsersCount { get; set; }

        public Coordinates Coordinates { get; set; }
    }
}