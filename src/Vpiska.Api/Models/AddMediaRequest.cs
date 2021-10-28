using System;
using Microsoft.AspNetCore.Http;

namespace Vpiska.Api.Models
{
    public sealed class AddMediaRequest
    {
        public Guid? EventId { get; set; }

        public IFormFile Media { get; set; }
    }
}