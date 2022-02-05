using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.RemoveRangeListenerCommand
{
    internal sealed class RemoveRangeListenerHandler : ICommandHandler<RemoveRangeListenerCommand>
    {
        private readonly ILogger<RemoveRangeListenerHandler> _logger;
        private readonly IUserConnectionsStorage _storage;

        public RemoveRangeListenerHandler(ILogger<RemoveRangeListenerHandler> logger,
            IUserConnectionsStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public Task HandleAsync(RemoveRangeListenerCommand command, CancellationToken cancellationToken = default)
        {
            if (!_storage.RemoveConnection(command.ConnectionId))
            {
                _logger.LogWarning("Can't remove user connection {}", command.ConnectionId);
            }
            
            return Task.CompletedTask;
        }
    }
}