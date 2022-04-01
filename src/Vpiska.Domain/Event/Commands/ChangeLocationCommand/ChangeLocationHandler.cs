using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Exceptions;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.ChangeLocationCommand
{
    internal sealed class ChangeLocationHandler : ICommandHandler<ChangeLocationCommand>
    {
        private readonly IValidator<ChangeLocationCommand> _validator;
        private readonly IEventRepository _repository;
        private readonly IEventStorage _eventStorage;
        private readonly IEventBus _eventBus;

        public ChangeLocationHandler(IValidator<ChangeLocationCommand> validator,
            IEventRepository repository,
            IEventStorage eventStorage,
            IEventBus eventBus)
        {
            _validator = validator;
            _repository = repository;
            _eventStorage = eventStorage;
            _eventBus = eventBus;
        }

        public async Task HandleAsync(ChangeLocationCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            var model = await _eventStorage.GetEvent(_repository, command.EventId, cancellationToken: cancellationToken);

            if (model == null)
            {
                throw new EventNotFoundException();
            }

            await _eventBus.PublishAsync(command.ToEvent(model.Users.Count));
        }
    }
}