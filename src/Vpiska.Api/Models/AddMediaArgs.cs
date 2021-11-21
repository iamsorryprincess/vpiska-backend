using Microsoft.AspNetCore.Http;

namespace Vpiska.Api.Models
{
    public sealed class AddMediaArgs
    {
        public string EventId { get; set; }

        public IFormFile Media { get; set; }
    }
}