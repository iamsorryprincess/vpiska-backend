using Microsoft.AspNetCore.Http;

namespace Vpiska.Api.Requests.Event
{
    public sealed class AddMediaRequest
    {
        public string EventId { get; set; }

        public IFormFile Media { get; set; }
    }
}