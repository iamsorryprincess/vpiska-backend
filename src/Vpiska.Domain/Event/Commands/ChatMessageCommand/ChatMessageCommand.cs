using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Commands.ChatMessageCommand
{
    public sealed class ChatMessageCommand
    {
        public string EventId { get; set; }

        public ChatMessage ChatMessage { get; set; }
    }
}