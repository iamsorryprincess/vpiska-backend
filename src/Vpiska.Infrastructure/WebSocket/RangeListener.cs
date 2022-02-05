using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Commands.AddRangeListenerCommand;
using Vpiska.Domain.Event.Commands.ChangeUserPositionCommand;
using Vpiska.Domain.Event.Commands.RemoveRangeListenerCommand;
using Vpiska.Domain.Interfaces;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class RangeListener : IWebSocketListener
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        public Task OnConnect(WebSocketContext socketContext)
        {
            var command = new AddRangeListenerCommand() { ConnectionId = socketContext.ConnectionId };
            var commandHandler = socketContext.ServiceProvider
                .GetRequiredService<ICommandHandler<AddRangeListenerCommand>>();
            return commandHandler.HandleAsync(command);
        }

        public Task OnDisconnect(WebSocketContext socketContext)
        {
            var command = new RemoveRangeListenerCommand() { ConnectionId = socketContext.ConnectionId };
            var commandHandler = socketContext.ServiceProvider
                .GetRequiredService<ICommandHandler<RemoveRangeListenerCommand>>();
            return commandHandler.HandleAsync(command);
        }

        public Task Receive(WebSocketContext socketContext, string route, string message)
        {
            switch (route)
            {
                case "changeRange":
                {
                    var command = new ChangeUserPositionCommand()
                    {
                        ConnectionId = socketContext.ConnectionId,
                        PositionInfo = JsonSerializer.Deserialize<PositionInfo>(message, JsonSerializerOptions)
                    };
                    var commandHandler = socketContext.ServiceProvider
                        .GetRequiredService<ICommandHandler<ChangeUserPositionCommand>>();
                    return commandHandler.HandleAsync(command);
                }
                default:
                    var logger = socketContext.ServiceProvider.GetRequiredService<ILogger<RangeListener>>();
                    logger.LogWarning("Unknown receiver route {}", route);
                    return Task.CompletedTask;
            }
        }
    }
}