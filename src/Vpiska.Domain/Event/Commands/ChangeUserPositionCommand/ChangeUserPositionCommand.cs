using System;

namespace Vpiska.Domain.Event.Commands.ChangeUserPositionCommand
{
    public sealed class ChangeUserPositionCommand
    {
        public Guid ConnectionId { get; set; }

        public PositionInfo PositionInfo { get; set; }
    }
}