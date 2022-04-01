using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Events.EventCreatedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Responses;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.CreateEventCommand
{
    internal sealed class CreateEventHandler : ICommandHandler<CreateEventCommand, EventResponse>
    {
        private readonly IValidator<CreateEventCommand> _validator;
        private readonly IEventRepository _repository;
        private readonly IEventBus _eventBus;

        public CreateEventHandler(IValidator<CreateEventCommand> validator,
            IEventRepository repository,
            IEventBus eventBus)
        {
            _validator = validator;
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task<EventResponse> HandleAsync(CreateEventCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            var isExist = await _repository.CheckByFieldAsync("ownerId", command.OwnerId, cancellationToken);

            if (isExist)
            {
                throw new OwnerAlreadyHasEventException();
            }

            var eventId = Guid.NewGuid().ToString();
            var model = command.ToModel(eventId);
            await _repository.InsertAsync(model, cancellationToken);
            var domainEvent = EventCreatedEvent.FromModel(model);
            await _eventBus.PublishAsync(domainEvent);
            return command.ToEventResponse(eventId);
        }
    }
}