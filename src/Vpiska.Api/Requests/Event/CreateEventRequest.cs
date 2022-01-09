namespace Vpiska.Api.Requests.Event
{
    public sealed class CreateEventRequest
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public CoordinatesDto Coordinates { get; set; }
    }
}