using System;
using MediatR;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.Requests
{
    public sealed class UpdateUserRequest : IRequest<DomainResponse>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }
        
        public byte[] ImageData { get; set; }
        
        public string ContentType { get; set; }
    }
}