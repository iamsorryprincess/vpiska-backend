using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Events.EventCreatedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Commands.CreateEventCommand
{
    internal sealed class CreateEventHandler : ValidationCommandHandler<CreateEventCommand, EventResponse>
    {
        private readonly IEventRepository _repository;
        private readonly IEventState _eventState;
        private readonly IEventBus _eventBus;

        public CreateEventHandler(IValidator<CreateEventCommand> validator,
            IEventRepository repository,
            IEventState eventState,
            IEventBus eventBus) : base(validator)
        {
            _repository = repository;
            _eventState = eventState;
            _eventBus = eventBus;
        }

        protected override async Task<EventResponse> Handle(CreateEventCommand command, CancellationToken cancellationToken)
        {
            var isExist = await _repository.CheckByFieldAsync("ownerId", command.OwnerId, cancellationToken);

            if (isExist)
            {
                throw new OwnerAlreadyHasEventException();
            }

            var eventId = Guid.NewGuid().ToString();
            var model = command.ToModel(eventId);
            await _repository.InsertAsync(model, cancellationToken);
            var domainEvent = EventCreatedEvent.FromModel(model);
            _eventBus.Publish(domainEvent);
            return command.ToEventResponse(eventId);
        }
    }
}