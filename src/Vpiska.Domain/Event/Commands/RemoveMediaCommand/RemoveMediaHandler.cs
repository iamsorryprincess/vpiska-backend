using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Events.MediaRemovedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.RemoveMediaCommand
{
    internal sealed class RemoveMediaHandler : ValidationCommandHandler<RemoveMediaCommand>
    {
        private readonly ICache<Event> _cache;
        private readonly IEventRepository _repository;
        private readonly IFileStorage _fileStorage;
        private readonly IEventStateManager _stateManager;
        private readonly IEventBus _eventBus;

        public RemoveMediaHandler(IValidator<RemoveMediaCommand> validator,
            ICache<Event> cache,
            IEventRepository repository,
            IFileStorage fileStorage,
            IEventStateManager stateManager,
            IEventBus eventBus) : base(validator)
        {
            _cache = cache;
            _repository = repository;
            _fileStorage = fileStorage;
            _stateManager = stateManager;
            _eventBus = eventBus;
        }

        protected override async Task Handle(RemoveMediaCommand command, CancellationToken cancellationToken)
        {
            var model = await _cache.GetData(command.EventId);

            if (model == null)
            {
                model = await _repository.GetByFieldAsync("_id", command.EventId, cancellationToken);
                
                if (model == null)
                {
                    throw new EventNotFoundException();
                }

                await _cache.SetData(model);
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
            await _stateManager.RemoveMediaLink(command.EventId, command.MediaId);
            await _repository.RemoveMediaLink(command.EventId, command.MediaId, cancellationToken);
            _eventBus.Publish(new MediaRemovedEvent() { EventId = command.EventId, MediaId = command.MediaId });
        }
    }
}