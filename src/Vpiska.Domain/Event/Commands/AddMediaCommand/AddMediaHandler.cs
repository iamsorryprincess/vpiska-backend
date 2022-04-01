using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.AddMediaCommand
{
    internal sealed class AddMediaHandler : ICommandHandler<AddMediaCommand>
    {
        private readonly IValidator<AddMediaCommand> _validator;
        private readonly IEventRepository _repository;
        private readonly IFileStorage _fileStorage;
        private readonly IEventStorage _eventStorage;
        private readonly IEventBus _eventBus;

        public AddMediaHandler(IValidator<AddMediaCommand> validator,
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

        public async Task HandleAsync(AddMediaCommand command, CancellationToken cancellationToken = default)
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

            var uploadResult = await _fileStorage.SaveFileAsync(Guid.NewGuid().ToString(), command.ContentType,
                command.MediaStream, cancellationToken);
            await _repository.AddMediaLink(command.EventId, uploadResult, cancellationToken);
            await _eventBus.PublishAsync(command.ToEvent(uploadResult));
        }
    }
}