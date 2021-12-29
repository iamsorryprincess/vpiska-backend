using Vpiska.Domain.Base;
using Vpiska.Domain.Models;

namespace Vpiska.Domain.Events
{
    public sealed class ChatMessageSent : ChatEvent
    {
        public ChatData ChatData { get; set; }
    }
}