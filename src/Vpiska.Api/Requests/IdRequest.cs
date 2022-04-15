using Vpiska.Domain.Event.Commands.CloseEventCommand;

namespace Vpiska.Api.Requests
{
    public sealed class IdRequest
    {
        public string EventId { get; set; }

        public CloseEventCommand ToCloseEventCommand(string ownerId) => new()
        {
            EventId = EventId,
            OwnerId = ownerId
        };
    }
}