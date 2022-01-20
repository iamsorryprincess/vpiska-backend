using Vpiska.Domain.Event.Commands.RemoveMediaCommand;

namespace Vpiska.Api.Requests
{
    public sealed class RemoveMediaRequest
    {
        public string EventId { get; set; }

        public string MediaId { get; set; }

        public RemoveMediaCommand ToCommand(string ownerId) => new RemoveMediaCommand()
        {
            EventId = EventId,
            OwnerId = ownerId,
            MediaId = MediaId
        };
    }
}