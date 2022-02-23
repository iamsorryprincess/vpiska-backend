using System.Collections.Generic;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Commands.CreateEventCommand
{
    public sealed class CreateEventCommand
    {
        public string OwnerId { get; set; }
        
        public string Name { get; set; }

        public string Address { get; set; }

        public CoordinatesDto Coordinates { get; set; }

        public Event ToModel(string eventId) => new(eventId, OwnerId, Name, Address, new Coordinates()
        {
            X = Coordinates.X.Value,
            Y = Coordinates.Y.Value
        }, new List<string>(), new List<ChatMessage>(), new List<UserInfo>());

        public EventResponse ToEventResponse(string eventId) => new()
        {
            Id = eventId,
            OwnerId = OwnerId,
            Name = Name,
            Address = Address,
            Coordinates = new Coordinates()
            {
                X = Coordinates.X.Value,
                Y = Coordinates.Y.Value
            }
        };
    }
}