using System;
using Vpiska.Domain.Event.Events.UserDisconnectedEvent;

namespace Vpiska.Domain.Event.Commands.RemoveUserCommand
{
    public sealed class RemoveUserCommand
    {
        public string EventId { get; set; }

        public Guid ConnectionId { get; set; }

        public string UserId { get; set; }

        public UserDisconnectedEvent ToEvent() => new()
        {
            EventId = EventId,
            UserId = UserId
        };
    }
}