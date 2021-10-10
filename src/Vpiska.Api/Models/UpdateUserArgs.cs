using Microsoft.AspNetCore.Http;

namespace Vpiska.Api.Models
{
    public sealed class UpdateUserArgs
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string Phone { get; set; }

        public IFormFile Image { get; set; }
    }
}