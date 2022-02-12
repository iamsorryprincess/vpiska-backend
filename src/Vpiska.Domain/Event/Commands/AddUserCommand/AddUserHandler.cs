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
        private readonly IEventConnectionsStorage _storage;
        private readonly IEventBus _eventBus;

        public AddUserHandler(ILogger<AddUserHandler> logger,
            IEventConnectionsStorage storage,
            IEventBus eventBus)
        {
            _logger = logger;
            _storage = storage;
            _eventBus = eventBus;
        }
        
        public Task HandleAsync(AddUserCommand command, CancellationToken cancellationToken = default)
        {
            if (!_storage.IsEventGroupExist(command.EventId))
            {
                if (!_storage.CreateEventGroup(command.EventId))
                {
                    _logger.LogWarning("Can't create event group for event: {}", command.EventId);
                    return Task.CompletedTask;
                }
            }

            if (!_storage.AddConnection(command.EventId, command.ConnectionId, command.UserInfo.UserId))
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