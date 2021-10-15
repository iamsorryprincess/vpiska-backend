using System;

namespace Vpiska.Domain.Requests
{
    public sealed class UpdateUserRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }
    }
}