using MediatR;
using Vpiska.Domain.Base;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.Requests
{
    public sealed class LoginUserRequest : IRequest<DomainResponse<LoginResponse>>
    {
        public string Phone { get; set; }

        public string Password { get; set; }
    }
}