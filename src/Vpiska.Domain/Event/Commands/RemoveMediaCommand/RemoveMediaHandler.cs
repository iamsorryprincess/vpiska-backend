using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.RemoveMediaCommand
{
    internal sealed class RemoveMediaHandler : ICommandHandler<RemoveMediaCommand>
    {
        private readonly IValidator<RemoveMediaCommand> _validator;
        private readonly IEventRepository _repository;
        private readonly IFileStorage _fileStorage;
        private readonly IEventStorage _eventStorage;
        private readonly IEventBus _eventBus;

        public RemoveMediaHandler(IValidator<RemoveMediaCommand> validator,
            IEventRepository repository,
            IFileStorage fileStorage,
            IEventStorage eventStorage,
            IEventBus eventBus)
        {
            _validator = validator;
            _repository = repository;
            _fileStorage = fileStorage;
            _eventStorage = eventStorage;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(RemoveMediaCommand command, CancellationToken cancellationToken = default)
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

            if (!model.MediaLinks.Contains(command.MediaId))
            {
                throw new MediaNotFoundException();
            }

            await _fileStorage.DeleteFileAsync(command.MediaId, cancellationToken);
            await _repository.RemoveMediaLink(command.EventId, command.MediaId, cancellationToken);
            await _eventBus.PublishAsync(command.ToEvent());
        }
    }
}