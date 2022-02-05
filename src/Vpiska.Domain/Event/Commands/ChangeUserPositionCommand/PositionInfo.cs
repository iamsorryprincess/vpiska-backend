using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Commands.ChangeUserPositionCommand
{
    public sealed class PositionInfo
    {
        public double HorizontalRange { get; set; }

        public double VerticalRange { get; set; }
        
        public Coordinates Coordinates { get; set; }
    }
}