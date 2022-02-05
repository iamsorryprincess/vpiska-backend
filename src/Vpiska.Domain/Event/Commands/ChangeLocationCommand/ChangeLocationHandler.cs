using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Events.EventUpdatedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.ChangeLocationCommand
{
    internal sealed class ChangeLocationHandler : ValidationCommandHandler<ChangeLocationCommand>
    {
        private readonly ICache<Event> _cache;
        private readonly IEventRepository _repository;
        private readonly IEventStateManager _eventStateManager;
        private readonly IEventBus _eventBus;

        public ChangeLocationHandler(IValidator<ChangeLocationCommand> validator,
            ICache<Event> cache,
            IEventRepository repository,
            IEventStateManager eventStateManager,
            IEventBus eventBus) : base(validator)
        {
            _cache = cache;
            _repository = repository;
            _eventStateManager = eventStateManager;
            _eventBus = eventBus;
        }

        protected override async Task Handle(ChangeLocationCommand command, CancellationToken cancellationToken)
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

            var newCoordinates = command.Coordinates.ToModel();
            await _eventStateManager.UpdateLocation(command.EventId, command.Address, newCoordinates);
            _eventBus.Publish(new EventUpdatedEvent()
            {
                EventId = command.EventId,
                Address = command.Address,
                UsersCount = model.Users.Count,
                Coordinates = newCoordinates
            });
        }
    }
}