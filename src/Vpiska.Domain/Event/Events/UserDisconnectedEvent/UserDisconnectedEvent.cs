using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.UserDisconnectedEvent
{
    public sealed class UserDisconnectedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public string UserId { get; set; }
    }
}