using System;

namespace Vpiska.Domain.UserAggregate.Responses
{
    public sealed class LoginResponse
    {
        public Guid UserId { get; set; }

        public string AccessToken { get; set; }
    }
}