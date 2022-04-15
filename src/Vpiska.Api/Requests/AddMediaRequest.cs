using Microsoft.AspNetCore.Http;
using Vpiska.Domain.Event.Commands.AddMediaCommand;

namespace Vpiska.Api.Requests
{
    public sealed class AddMediaRequest
    {
        public string EventId { get; set; }

        public IFormFile Media { get; set; }

        public AddMediaCommand ToCommand(string ownerId) => new()
        {
            EventId = EventId,
            OwnerId = ownerId,
            ContentType = Media?.ContentType,
            MediaStream = Media?.OpenReadStream()
        };
    }
}