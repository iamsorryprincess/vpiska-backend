using MediatR;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.Requests
{
    public sealed class CheckCodeRequest : IRequest<DomainResponse<LoginResponse>>
    {
        public string Phone { get; set; }

        public int? Code { get; set; }
    }
}