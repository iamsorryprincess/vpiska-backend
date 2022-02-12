using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Events.EventUpdatedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Commands.ChangeLocationCommand
{
    internal sealed class ChangeLocationHandler : ValidationCommandHandler<ChangeLocationCommand>
    {
        private readonly IEventRepository _repository;
        private readonly IEventState _eventState;
        private readonly IEventBus _eventBus;

        public ChangeLocationHandler(IValidator<ChangeLocationCommand> validator,
            IEventRepository repository,
            IEventState eventState,
            IEventBus eventBus) : base(validator)
        {
            _repository = repository;
            _eventState = eventState;
            _eventBus = eventBus;
        }

        protected override async Task Handle(ChangeLocationCommand command, CancellationToken cancellationToken)
        {
            var model = await _eventState.GetEvent(_repository, command.EventId, cancellationToken: cancellationToken);

            if (model == null)
            {
                throw new EventNotFoundException();
            }
            
            var newCoordinates = command.Coordinates.ToModel();
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