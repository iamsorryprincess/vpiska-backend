using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.MediaAddedEvent
{
    public sealed class MediaAddedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public MediaInfo MediaInfo { get; set; }
    }
}