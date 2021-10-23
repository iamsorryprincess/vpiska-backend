using System;
using MediatR;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.Requests
{
    public sealed class CreateEventRequest : IRequest<DomainResponse<EventResponse>>
    {
        public Guid OwnerId { get; set; }
        
        public string Name { get; set; }

        public string Coordinates { get; set; }

        public string Address { get; set; }

        public string Area { get; set; }
    }
}