using System;

namespace Vpiska.Domain.Requests
{
    public sealed class ChangePasswordRequest
    {
        public Guid Id { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}