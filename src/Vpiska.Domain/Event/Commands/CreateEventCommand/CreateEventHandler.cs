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
    internal sealed class CreateEventHandler : ValidationCommandHandler<CreateEventCommand, EventResponse>
    {
        private readonly IEventRepository _repository;
        private readonly ICache<Event> _cache;
        private readonly IEventBus _eventBus;

        public CreateEventHandler(IValidator<CreateEventCommand> validator,
            IEventRepository repository,
            ICache<Event> cache,
            IEventBus eventBus) : base(validator)
        {
            _repository = repository;
            _cache = cache;
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
            await _cache.SetData(model);
            var domainEvent = EventCreatedEvent.FromModel(model);
            _eventBus.Publish(domainEvent);
            return command.ToEventResponse(eventId);
        }
    }
}