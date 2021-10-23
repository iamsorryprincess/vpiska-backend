using MediatR;
using Vpiska.Domain.Base;

namespace Vpiska.Domain.UserAggregate.Requests
{
    public sealed class SetCodeRequest : IRequest<DomainResponse>
    {
        public string Phone { get; set; }

        public string FirebaseToken { get; set; }
    }
}