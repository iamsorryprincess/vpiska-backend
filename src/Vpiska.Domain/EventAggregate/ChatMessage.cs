using System;

namespace Vpiska.Domain.EventAggregate
{
    public sealed class ChatMessage
    {
        public Guid UserId { get; }

        public string Message { get; }

        public ChatMessage(Guid userId, string message)
        {
            UserId = userId;
            Message = message;
        }
    }
}