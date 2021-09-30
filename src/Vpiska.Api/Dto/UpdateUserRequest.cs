using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Vpiska.Api.Dto
{
    public sealed class UpdateUserRequest
    {
        [Required(ErrorMessage = "Id обязателен")]
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string Phone { get; set; }

        public IFormFile Image { get; set; }
    }
}