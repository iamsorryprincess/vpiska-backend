using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Responses
{
    public sealed class EventShortResponse
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Coordinates Coordinates { get; set; }

        public int UsersCount { get; set; }

        public static EventShortResponse FromModel(Event model) => new EventShortResponse()
        {
            Id = model.Id,
            Name = model.Name,
            Coordinates = model.Coordinates,
            UsersCount = model.Users.Count
        };
    }
}