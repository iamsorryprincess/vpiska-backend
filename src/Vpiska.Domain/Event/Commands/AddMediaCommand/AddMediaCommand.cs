using System.IO;

namespace Vpiska.Domain.Event.Commands.AddMediaCommand
{
    public sealed class AddMediaCommand
    {
        public string EventId { get; set; }

        public string OwnerId { get; set; }

        public string ContentType { get; set; }

        public Stream MediaStream { get; set; }
    }
}