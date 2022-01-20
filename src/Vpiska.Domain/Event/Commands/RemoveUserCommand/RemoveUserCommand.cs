using System;

namespace Vpiska.Domain.Event.Commands.RemoveUserCommand
{
    public sealed class RemoveUserCommand
    {
        public string EventId { get; set; }

        public Guid ConnectionId { get; set; }

        public string UserId { get; set; }
    }
}