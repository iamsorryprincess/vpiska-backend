using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Events.MediaAddedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.AddMediaCommand
{
    internal sealed class AddMediaHandler : ValidationCommandHandler<AddMediaCommand>
    {
        private readonly ICache<Event> _cache;
        private readonly IEventRepository _repository;
        private readonly IFileStorage _fileStorage;
        private readonly IEventStateManager _stateManager;
        private readonly IEventBus _eventBus;

        public AddMediaHandler(IValidator<AddMediaCommand> validator,
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

        protected override async Task Handle(AddMediaCommand command, CancellationToken cancellationToken)
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

            var uploadResult = await _fileStorage.SaveFileAsync(Guid.NewGuid().ToString(), command.ContentType,
                command.MediaStream, cancellationToken);
            await _stateManager.AddMediaLink(command.EventId, uploadResult);
            await _repository.AddMediaLink(command.EventId, uploadResult, cancellationToken);
            _eventBus.Publish(new MediaAddedEvent() { EventId = command.EventId, MediaId = uploadResult });
        }
    }
}