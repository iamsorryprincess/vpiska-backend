using Vpiska.Domain.Event.Commands.CreateEventCommand;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Api.Requests
{
    public sealed class CreateEventRequest
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public CoordinatesDto Coordinates { get; set; }

        public CreateEventCommand ToCommand(string ownerId) => new CreateEventCommand()
        {
            OwnerId = ownerId,
            Name = Name,
            Address = Address,
            Coordinates = Coordinates
        };
    }
}