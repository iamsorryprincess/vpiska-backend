using System.IO;
using Vpiska.Domain.Event.Events.MediaAddedEvent;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Commands.AddMediaCommand
{
    public sealed class AddMediaCommand
    {
        public string EventId { get; set; }

        public string OwnerId { get; set; }

        public string ContentType { get; set; }

        public Stream MediaStream { get; set; }

        public MediaAddedEvent ToEvent(MediaInfo mediaInfo) => new()
        {
            EventId = EventId,
            MediaInfo = mediaInfo
        };
    }
}