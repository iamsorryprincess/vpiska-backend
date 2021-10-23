using System;
using System.Collections.Generic;

namespace Vpiska.Domain.EventAggregate.Responses
{
    public sealed class EventResponse
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public string Coordinates { get; set; }

        public string Address { get; set; }
        
        public IReadOnlyList<UserInfo> Users { get; set; }

        public IReadOnlyList<string> MediaLinks { get; set; }
    }
}