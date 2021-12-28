using Vpiska.Domain.Models;

namespace Vpiska.Api.Responses.Event
{
    public sealed class EventShortResponse
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Coordinates Coordinates { get; set; }

        public int UsersCount { get; set; }
    }
}