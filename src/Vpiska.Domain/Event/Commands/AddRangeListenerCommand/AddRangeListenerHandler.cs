using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Commands.AddRangeListenerCommand
{
    internal sealed class AddRangeListenerHandler : ICommandHandler<AddRangeListenerCommand>
    {
        private readonly IUserConnectionsStorage _storage;

        public AddRangeListenerHandler(IUserConnectionsStorage storage)
        {
            _storage = storage;
        }

        public Task HandleAsync(AddRangeListenerCommand command, CancellationToken cancellationToken = default)
        {
            _storage.AddConnection(command.ConnectionId);
            return Task.CompletedTask;
        }
    }
}