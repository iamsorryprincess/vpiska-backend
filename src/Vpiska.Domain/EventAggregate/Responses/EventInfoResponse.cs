using System;

namespace Vpiska.Domain.EventAggregate.Responses
{
    public sealed class EventInfoResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Coordinates { get; set; }

        public int UsersCount { get; set; }
    }
}