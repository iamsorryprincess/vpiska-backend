namespace Vpiska.Api.Requests.Event
{
    public sealed class GetEventsRequest
    {
        public double? Range { get; set; }

        public CoordinatesRequest Coordinates { get; set; }
    }
}