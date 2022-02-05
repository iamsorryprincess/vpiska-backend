using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.ChangeUserPositionCommand
{
    internal sealed class ChangeUserPositionHandler : ICommandHandler<ChangeUserPositionCommand>
    {
        private readonly ILogger<ChangeUserPositionHandler> _logger;
        private readonly IUserConnectionsStorage _storage;

        public ChangeUserPositionHandler(ILogger<ChangeUserPositionHandler> logger,
            IUserConnectionsStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public Task HandleAsync(ChangeUserPositionCommand command, CancellationToken cancellationToken = default)
        {
            if (!_storage.SetRange(command.ConnectionId, command.PositionInfo.Coordinates.X,
                command.PositionInfo.Coordinates.Y, command.PositionInfo.HorizontalRange,
                command.PositionInfo.VerticalRange))
            {
                _logger.LogWarning("can't update user connection range. Id - {}", command.ConnectionId);
            }
            
            return Task.CompletedTask;
        }
    }
}