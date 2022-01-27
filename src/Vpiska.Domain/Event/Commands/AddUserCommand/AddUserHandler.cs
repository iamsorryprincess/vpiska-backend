using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Events.UserConnectedEvent;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.AddUserCommand
{
    internal sealed class AddUserHandler : ICommandHandler<AddUserCommand>
    {
        private readonly ILogger<AddUserHandler> _logger;
        private readonly IConnectionsStorage _storage;
        private readonly IEventStateManager _stateManager;
        private readonly IEventSender _eventSender;
        private readonly IEventBus _eventBus;

        public AddUserHandler(ILogger<AddUserHandler> logger,
            IConnectionsStorage storage,
            IEventStateManager stateManager,
            IEventSender eventSender,
            IEventBus eventBus)
        {
            _logger = logger;
            _storage = storage;
            _stateManager = stateManager;
            _eventSender = eventSender;
            _eventBus = eventBus;
        }
        
        public async Task HandleAsync(AddUserCommand command, CancellationToken cancellationToken = default)
        {
            if (!_storage.IsEventGroupExist(command.EventId))
            {
                if (!_storage.CreateEventGroup(command.EventId))
                {
                    _logger.LogWarning("Can't create event group for event: {}", command.EventId);
                    return;
                }
            }

            if (!_storage.AddConnection(command.EventId, command.ConnectionId, command.UserInfo.UserId))
            {
                _logger.LogWarning("Can't add user connection. EventId: {}, UserId: {}", command.EventId,
                    command.UserInfo.UserId);
                return;
            }
            
            await _stateManager.AddUserInfo(command.EventId, command.UserInfo);
            var chatMessages = await _stateManager.GetChatMessages(command.EventId);

            if (chatMessages.Any())
            {
                foreach (var chatMessage in chatMessages)
                {
                    await _eventSender.SendChatMessageToUser(command.ConnectionId, chatMessage);
                }
            }

            var domainEvent = new UserConnectedEvent() { EventId = command.EventId, UserInfo = command.UserInfo };
            _eventBus.Publish(domainEvent);
        }
    }
}