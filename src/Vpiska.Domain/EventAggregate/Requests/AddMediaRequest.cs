using System;
using MediatR;
using Vpiska.Domain.Base;
using Vpiska.Domain.EventAggregate.Responses;

namespace Vpiska.Domain.EventAggregate.Requests
{
    public sealed class AddMediaRequest : IRequest<DomainResponse<MediaResponse>>
    {
        public Guid EventId { get; set; }

        public Guid OwnerId { get; set; }

        public byte[] MediaData { get; set; }

        public string ContentType { get; set; }
    }
}