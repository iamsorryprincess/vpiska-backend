using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.AddUserCommand
{
    internal sealed class AddUserHandler : ICommandHandler<AddUserCommand>
    {
        private readonly ILogger<AddUserHandler> _logger;
        private readonly IEventStorage _eventStorage;
        private readonly IEventRepository _repository;
        private readonly IEventConnectionsStorage _eventConnectionsStorage;
        private readonly IEventBus _eventBus;

        public AddUserHandler(ILogger<AddUserHandler> logger,
            IEventStorage eventStorage,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventBus eventBus)
        {
            _logger = logger;
            _eventStorage = eventStorage;
            _repository = repository;
            _eventConnectionsStorage = eventConnectionsStorage;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(AddUserCommand command, CancellationToken cancellationToken = default)
        {
            var model = await _eventStorage.GetEvent(_repository, command.EventId, cancellationToken: cancellationToken);

            if (model == null)
            {
                throw new EventNotFoundException();
            }
            
            if (!_eventConnectionsStorage.IsEventGroupExist(command.EventId))
            {
                if (!_eventConnectionsStorage.CreateEventGroup(command.EventId))
                {
                    _logger.LogWarning("Can't create event group for event: {}", command.EventId);
                    throw new EventGroupConnectionException();
                }
            }

            if (!_eventConnectionsStorage.AddConnection(command.EventId, command.ConnectionId, command.UserInfo.UserId))
            {
                _logger.LogWarning("Can't add user connection. EventId: {}, UserId: {}", command.EventId,
                    command.UserInfo.UserId);
                throw new UserToEventConnectionException();
            }
            
            _eventBus.Publish(command.ToEvent());
        }
    }
}