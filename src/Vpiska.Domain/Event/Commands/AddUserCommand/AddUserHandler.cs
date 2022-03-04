using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.AddUserCommand
{
    internal sealed class AddUserHandler : ICommandHandler<AddUserCommand>
    {
        private readonly ILogger<AddUserHandler> _logger;
        private readonly IEventConnectionsStorage _eventConnectionsStorage;
        private readonly IEventBus _eventBus;

        public AddUserHandler(ILogger<AddUserHandler> logger,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventBus eventBus)
        {
            _logger = logger;
            _eventConnectionsStorage = eventConnectionsStorage;
            _eventBus = eventBus;
        }
        
        public Task HandleAsync(AddUserCommand command, CancellationToken cancellationToken = default)
        {
            if (!_eventConnectionsStorage.IsEventGroupExist(command.EventId))
            {
                if (!_eventConnectionsStorage.CreateEventGroup(command.EventId))
                {
                    _logger.LogWarning("Can't create event group for event: {}", command.EventId);
                    return Task.CompletedTask;
                }
            }

            if (!_eventConnectionsStorage.AddConnection(command.EventId, command.ConnectionId, command.UserInfo.UserId))
            {
                _logger.LogWarning("Can't add user connection. EventId: {}, UserId: {}", command.EventId,
                    command.UserInfo.UserId);
                return Task.CompletedTask;
            }
            
            _eventBus.Publish(command.ToEvent());
            return Task.CompletedTask;
        }
    }
}