using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.RemoveUserCommand
{
    internal sealed class RemoveUserHandler : ICommandHandler<RemoveUserCommand>
    {
        private readonly ILogger<RemoveUserHandler> _logger;
        private readonly IEventConnectionsStorage _storage;
        private readonly IEventBus _eventBus;

        public RemoveUserHandler(ILogger<RemoveUserHandler> logger,
            IEventConnectionsStorage storage,
            IEventBus eventBus)
        {
            _logger = logger;
            _storage = storage;
            _eventBus = eventBus;
        }
        
        public Task HandleAsync(RemoveUserCommand command, CancellationToken cancellationToken = default)
        {
            if (!_storage.RemoveConnection(command.EventId, command.ConnectionId))
            {
                _logger.LogWarning("Can't remove connection. EventId: {}, UserId: {}", command.EventId,
                    command.UserId);
                return Task.CompletedTask;
            }
            
            _eventBus.Publish(command.ToEvent());
            return Task.CompletedTask;
        }
    }
}