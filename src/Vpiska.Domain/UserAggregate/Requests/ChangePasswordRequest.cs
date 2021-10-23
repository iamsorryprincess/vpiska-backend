using System;
using MediatR;
using Vpiska.Domain.Base;

namespace Vpiska.Domain.UserAggregate.Requests
{
    public sealed class ChangePasswordRequest : IRequest<DomainResponse>
    {
        public Guid Id { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}