using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Events.EventClosedEvent;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.CloseEventCommand
{
    internal sealed class CloseEventHandler : ValidationCommandHandler<CloseEventCommand>
    {
        private readonly ICache<Event> _cache;
        private readonly IEventRepository _repository;
        private readonly IEventBus _eventBus;

        public CloseEventHandler(IValidator<CloseEventCommand> validator,
            ICache<Event> cache,
            IEventRepository repository,
            IEventBus eventBus) : base(validator)
        {
            _cache = cache;
            _repository = repository;
            _eventBus = eventBus;
        }

        protected override async Task Handle(CloseEventCommand command, CancellationToken cancellationToken)
        {
            var model = await _cache.GetData(command.EventId);

            if (model == null)
            {
                model = await _repository.GetByFieldAsync("_id", command.EventId, cancellationToken);
                if (model == null)
                {
                    throw new EventNotFoundException();
                }
            }

            if (model.OwnerId != command.OwnerId)
            {
                throw new UserIsNotOwnerException();
            }
            
            _eventBus.Publish(new EventClosedEvent() { EventId = command.EventId });
            await _repository.RemoveByFieldAsync("_id", command.EventId, cancellationToken);
            await _cache.RemoveData(command.EventId);
        }
    }
}