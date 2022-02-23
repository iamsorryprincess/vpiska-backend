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
        private readonly IEventStorage _eventStorage;

        public ChatMessageHandler(IEventConnectionsStorage storage,
            IEventSender eventSender,
            IEventRepository repository,
            IEventStorage eventStorage)
        {
            _storage = storage;
            _eventSender = eventSender;
            _repository = repository;
            _eventStorage = eventStorage;
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

            var model = await _eventStorage.GetEvent(_repository, domainEvent.EventId);

            if (model == null)
            {
                return;
            }

            await _eventStorage.AddChatMessage(domainEvent.EventId, domainEvent.ChatMessage);
        }
    }
}