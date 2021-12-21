using Microsoft.AspNetCore.Http;

namespace Vpiska.Api.Requests
{
    public sealed class UpdateUserRequest
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public IFormFile Image { get; set; }
    }
}