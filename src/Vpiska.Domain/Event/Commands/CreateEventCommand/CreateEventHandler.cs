using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
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
        
        public CreateEventHandler(IValidator<CreateEventCommand> validator,
            IEventRepository repository,
            ICache<Event> cache) : base(validator)
        {
            _repository = repository;
            _cache = cache;
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
            return command.ToEventResponse(eventId);
        }
    }
}