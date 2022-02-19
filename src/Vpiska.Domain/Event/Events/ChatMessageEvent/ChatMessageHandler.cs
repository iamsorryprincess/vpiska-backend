using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.ChatMessageEvent
{
    internal sealed class ChatMessageHandler : IEventHandler<ChatMessageEvent>
    {
        private readonly IEventConnectionsStorage _storage;
        private readonly IEventSender _eventSender;
        private readonly IEventRepository _repository;
        private readonly IEventStorage _eventState;

        public ChatMessageHandler(IEventConnectionsStorage storage,
            IEventSender eventSender,
            IEventRepository repository,
            IEventStorage eventState)
        {
            _storage = storage;
            _eventSender = eventSender;
            _repository = repository;
            _eventState = eventState;
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

            var model = await _eventState.GetEvent(_repository, domainEvent.EventId);

            if (model == null)
            {
                return;
            }

            await _eventState.AddChatMessage(domainEvent.EventId, domainEvent.ChatMessage);
        }
    }
}