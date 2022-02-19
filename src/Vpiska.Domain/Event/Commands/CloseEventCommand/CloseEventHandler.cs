using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Events.EventClosedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Commands.CloseEventCommand
{
    internal sealed class CloseEventHandler : ValidationCommandHandler<CloseEventCommand>
    {
        private readonly IEventStorage _eventState;
        private readonly IEventRepository _repository;
        private readonly IEventBus _eventBus;

        public CloseEventHandler(IValidator<CloseEventCommand> validator,
            IEventStorage eventState,
            IEventRepository repository,
            IEventBus eventBus) : base(validator)
        {
            _eventState = eventState;
            _repository = repository;
            _eventBus = eventBus;
        }

        protected override async Task Handle(CloseEventCommand command, CancellationToken cancellationToken)
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

            _eventBus.Publish(new EventClosedEvent() { EventId = command.EventId, Coordinates = model.Coordinates });
            await _repository.RemoveByFieldAsync("_id", command.EventId, cancellationToken);
        }
    }
}