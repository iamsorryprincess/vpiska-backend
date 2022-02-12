using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Events.MediaAddedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.AddMediaCommand
{
    internal sealed class AddMediaHandler : ValidationCommandHandler<AddMediaCommand>
    {
        private readonly IEventRepository _repository;
        private readonly IFileStorage _fileStorage;
        private readonly IEventState _eventState;
        private readonly IEventBus _eventBus;

        public AddMediaHandler(IValidator<AddMediaCommand> validator,
            IEventRepository repository,
            IFileStorage fileStorage,
            IEventState eventState,
            IEventBus eventBus) : base(validator)
        {
            _repository = repository;
            _fileStorage = fileStorage;
            _eventState = eventState;
            _eventBus = eventBus;
        }

        protected override async Task Handle(AddMediaCommand command, CancellationToken cancellationToken)
        {
            var model = await _eventState.GetEvent(_repository, command.EventId, cancellationToken: cancellationToken);

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
            _eventBus.Publish(new MediaAddedEvent() { EventId = command.EventId, MediaId = uploadResult });
        }
    }
}