using Microsoft.AspNetCore.Http;
using Vpiska.Domain.User.Commands.UpdateUserCommand;

namespace Vpiska.Api.Requests
{
    public sealed class UpdateUserRequest
    {
        public string Id { get; set; }

        public string Phone { get; set; }

        public string Name { get; set; }

        public IFormFile Image { get; set; }

        public UpdateUserCommand ToCommand() => new()
        {
            Id = Id,
            Phone = Phone,
            Name = Name,
            ContentType = Image?.ContentType,
            ImageStream = Image?.OpenReadStream()
        };
    }
}