using System;

namespace Vpiska.Domain.Responses
{
    public sealed class LoginResponse
    {
        public Guid UserId { get; set; }

        public string AccessToken { get; set; }
    }
}