using MediatR;
using Vpiska.Domain.Base;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.Requests
{
    public sealed class CreateUserRequest : IRequest<DomainResponse<LoginResponse>>
    {
        public string Phone { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}