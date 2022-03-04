using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.CloseEventCommand
{
    internal sealed class CloseEventHandler : ICommandHandler<CloseEventCommand>
    {
        private readonly IValidator<CloseEventCommand> _validator;
        private readonly IEventStorage _eventStorage;
        private readonly IEventRepository _repository;
        private readonly IEventBus _eventBus;

        public CloseEventHandler(IValidator<CloseEventCommand> validator,
            IEventStorage eventStorage,
            IEventRepository repository,
            IEventBus eventBus)
        {
            _validator = validator;
            _eventStorage = eventStorage;
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(CloseEventCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            var model = await _eventStorage.GetEvent(_repository, command.EventId, cancellationToken: cancellationToken);

            if (model == null)
            {
                throw new EventNotFoundException();
            }

            if (model.OwnerId != command.OwnerId)
            {
                throw new UserIsNotOwnerException();
            }

            _eventBus.Publish(command.ToEvent(model.Coordinates));
            await _repository.RemoveByFieldAsync("_id", command.EventId, cancellationToken);
        }
    }
}