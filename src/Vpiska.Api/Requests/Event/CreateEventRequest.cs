using Vpiska.Domain.Models;

namespace Vpiska.Api.Requests.Event
{
    public sealed class CreateEventRequest
    {
        public string OwnerId { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public Coordinates Coordinates { get; set; }
    }
}