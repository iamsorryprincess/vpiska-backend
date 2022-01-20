using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.ChatMessageEvent
{
    internal sealed class ChatMessageHandler : IEventHandler<ChatMessageEvent>
    {
        private readonly IConnectionsStorage _storage;
        private readonly IEventSender _eventSender;

        public ChatMessageHandler(IConnectionsStorage storage, IEventSender eventSender)
        {
            _storage = storage;
            _eventSender = eventSender;
        }
        
        public async Task Handle(ChatMessageEvent domainEvent)
        {
            if (_storage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _storage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await _eventSender.SendChatMessageToConnections(connections, domainEvent.ChatMessage);
                }
            }
        }
    }
}