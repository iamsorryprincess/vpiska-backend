using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Queries.GetEventsQuery
{
    public sealed class GetEventsQuery
    {
        public double? HorizontalRange { get; set; }

        public double? VerticalRange { get; set; }

        public CoordinatesDto Coordinates { get; set; }
    }
}