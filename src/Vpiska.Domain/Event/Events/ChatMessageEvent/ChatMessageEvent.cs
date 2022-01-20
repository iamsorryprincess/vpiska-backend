using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.ChatMessageEvent
{
    public sealed class ChatMessageEvent : IDomainEvent
    {
        public string EventId { get; set; }
        
        public ChatMessage ChatMessage { get; set; }
    }
}