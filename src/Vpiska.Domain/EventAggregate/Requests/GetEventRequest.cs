using System;
using MediatR;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.Requests
{
    public sealed class GetEventRequest : IRequest<DomainResponse<EventResponse>>
    {
        public Guid Id { get; set; }
    }
}