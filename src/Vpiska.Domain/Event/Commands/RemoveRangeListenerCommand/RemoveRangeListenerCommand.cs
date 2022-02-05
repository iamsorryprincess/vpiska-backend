using System;

namespace Vpiska.Domain.Event.Commands.RemoveRangeListenerCommand
{
    public sealed class RemoveRangeListenerCommand
    {
        public Guid ConnectionId { get; set; }
    }
}