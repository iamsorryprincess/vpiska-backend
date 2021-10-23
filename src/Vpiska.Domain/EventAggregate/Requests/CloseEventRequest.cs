using System;
using MediatR;
using Vpiska.Domain.Base;

namespace Vpiska.Domain.EventAggregate.Requests
{
    public sealed class CloseEventRequest : IRequest<DomainResponse>
    {
        public Guid EventId { get; set; }

        public Guid OwnerId { get; set; }
    }
}