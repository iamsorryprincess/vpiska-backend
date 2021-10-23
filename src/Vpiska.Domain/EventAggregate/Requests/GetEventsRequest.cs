using MediatR;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.Requests
{
    public sealed class GetEventsRequest : IRequest<DomainResponse<EventInfoResponse[]>>
    {
        public string Area { get; set; }
    }
}