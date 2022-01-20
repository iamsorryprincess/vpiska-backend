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

        public Event ToModel(string eventId) => new Event()
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

        public EventResponse ToEventResponse(string eventId) => new EventResponse()
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