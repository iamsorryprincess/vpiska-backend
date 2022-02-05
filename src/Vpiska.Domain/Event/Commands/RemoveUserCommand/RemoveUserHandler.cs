using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Events.UserDisconnectedEvent;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.RemoveUserCommand
{
    internal sealed class RemoveUserHandler : ICommandHandler<RemoveUserCommand>
    {
        private readonly ILogger<RemoveUserHandler> _logger;
        private readonly IEventConnectionsStorage _storage;
        private readonly IEventStateManager _stateManager;
        private readonly IEventBus _eventBus;

        public RemoveUserHandler(ILogger<RemoveUserHandler> logger,
            IEventConnectionsStorage storage,
            IEventStateManager stateManager,
            IEventBus eventBus)
        {
            _logger = logger;
            _storage = storage;
            _stateManager = stateManager;
            _eventBus = eventBus;
        }
        
        public async Task HandleAsync(RemoveUserCommand command, CancellationToken cancellationToken = default)
        {
            if (!_storage.RemoveConnection(command.EventId, command.ConnectionId))
            {
                _logger.LogWarning("Can't remove connection. EventId: {0}, UserId: {1}", command.EventId,
                    command.UserId);
                return;
            }

            await _stateManager.RemoveUserInfo(command.EventId, command.UserId);
            var domainEvent = new UserDisconnectedEvent() { EventId = command.EventId, UserId = command.UserId };
            _eventBus.Publish(domainEvent);
        }
    }
}