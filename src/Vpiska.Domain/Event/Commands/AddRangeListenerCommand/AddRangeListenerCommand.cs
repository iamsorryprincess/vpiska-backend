using System;

namespace Vpiska.Domain.Event.Commands.AddRangeListenerCommand
{
    public sealed class AddRangeListenerCommand
    {
        public Guid ConnectionId { get; set; }
    }
}