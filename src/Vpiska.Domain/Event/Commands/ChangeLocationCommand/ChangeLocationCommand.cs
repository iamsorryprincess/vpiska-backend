using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Commands.ChangeLocationCommand
{
    public sealed class ChangeLocationCommand
    {
        public string EventId { get; set; }

        public string Address { get; set; }

        public CoordinatesDto Coordinates { get; set; }
    }
}