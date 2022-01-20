using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Events.ChatMessageEvent;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.ChatMessageCommand
{
    internal sealed class ChatMessageHandler : ICommandHandler<ChatMessageCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly IEventStateManager _stateManager;

        public ChatMessageHandler(IEventBus eventBus, IEventStateManager stateManager)
        {
            _eventBus = eventBus;
            _stateManager = stateManager;
        }
        
        public Task HandleAsync(ChatMessageCommand command, CancellationToken cancellationToken = default)
        {
            _eventBus.Publish(new ChatMessageEvent() { EventId = command.EventId, ChatMessage = command.ChatMessage });
            return _stateManager.AddChatMessage(command.EventId, command.ChatMessage);
        }
    }
}