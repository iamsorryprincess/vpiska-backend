namespace Vpiska.Api.Requests.Event
{
    public sealed class GetEventsRequest
    {
        public double? HorizontalRange { get; set; }

        public double? VerticalRange { get; set; }

        public CoordinatesDto Coordinates { get; set; }
    }
}