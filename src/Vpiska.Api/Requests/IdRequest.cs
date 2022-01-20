using Vpiska.Domain.Event.Commands.CloseEventCommand;

namespace Vpiska.Api.Requests
{
    public sealed class IdRequest
    {
        public string EventId { get; set; }

        public CloseEventCommand ToCloseEventCommand(string ownerId) => new CloseEventCommand()
        {
            EventId = EventId,
            OwnerId = ownerId
        };
    }
}