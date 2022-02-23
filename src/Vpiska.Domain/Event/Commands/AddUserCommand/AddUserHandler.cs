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
        private readonly IEventConnectionsStorage _userStorage;
        private readonly IEventBus _eventBus;

        public AddUserHandler(ILogger<AddUserHandler> logger,
            IEventConnectionsStorage userStorage,
            IEventBus eventBus)
        {
            _logger = logger;
            _userStorage = userStorage;
            _eventBus = eventBus;
        }
        
        public Task HandleAsync(AddUserCommand command, CancellationToken cancellationToken = default)
        {
            if (!_userStorage.IsEventGroupExist(command.EventId))
            {
                if (!_userStorage.CreateEventGroup(command.EventId))
                {
                    _logger.LogWarning("Can't create event group for event: {}", command.EventId);
                    return Task.CompletedTask;
                }
            }

            if (!_userStorage.AddConnection(command.EventId, command.ConnectionId, command.UserInfo.UserId))
            {
                _logger.LogWarning("Can't add user connection. EventId: {}, UserId: {}", command.EventId,
                    command.UserInfo.UserId);
                return Task.CompletedTask;
            }
            
            var domainEvent = new UserConnectedEvent() { EventId = command.EventId, UserInfo = command.UserInfo };
            _eventBus.Publish(domainEvent);
            return Task.CompletedTask;
        }
    }
}