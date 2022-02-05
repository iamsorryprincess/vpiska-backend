using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Events.EventCreatedEvent
{
    public sealed class EventCreatedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public string Name { get; set; }

        public int UsersCount { get; set; }
        
        public Coordinates Coordinates { get; set; }

        public EventShortResponse ToShortResponse() => new()
        {
            Id = EventId,
            Name = Name,
            Coordinates = Coordinates,
            UsersCount = UsersCount
        };

        public static EventCreatedEvent FromModel(Event model) => new()
        {
            EventId = model.Id,
            Name = model.Name,
            Coordinates = model.Coordinates,
            UsersCount = model.Users.Count
        };
    }
}