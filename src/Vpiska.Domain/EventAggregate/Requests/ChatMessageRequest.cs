using System;
using MediatR;
using Vpiska.Domain.Base;

namespace Vpiska.Domain.EventAggregate.Requests
{
    public sealed class ChatMessageRequest : IRequest<DomainResponse>
    {
        public Guid EventId { get; set; }
        
        public Guid UserId { get; set; }

        public string Message { get; set; }
    }
}