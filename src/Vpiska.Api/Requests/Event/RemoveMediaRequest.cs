namespace Vpiska.Api.Requests.Event
{
    public sealed class RemoveMediaRequest
    {
        public string EventId { get; set; }

        public string MediaId { get; set; }
    }
}