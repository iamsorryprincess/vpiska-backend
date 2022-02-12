using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Events.MediaRemovedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.RemoveMediaCommand
{
    internal sealed class RemoveMediaHandler : ValidationCommandHandler<RemoveMediaCommand>
    {
        private readonly IEventRepository _repository;
        private readonly IFileStorage _fileStorage;
        private readonly IEventState _eventState;
        private readonly IEventBus _eventBus;

        public RemoveMediaHandler(IValidator<RemoveMediaCommand> validator,
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

        protected override async Task Handle(RemoveMediaCommand command, CancellationToken cancellationToken)
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

            if (!model.MediaLinks.Contains(command.MediaId))
            {
                throw new MediaNotFoundException();
            }

            await _fileStorage.DeleteFileAsync(command.MediaId, cancellationToken);
            await _repository.RemoveMediaLink(command.EventId, command.MediaId, cancellationToken);
            _eventBus.Publish(new MediaRemovedEvent() { EventId = command.EventId, MediaId = command.MediaId });
        }
    }
}