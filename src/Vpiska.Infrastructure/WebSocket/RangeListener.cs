using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Commands.ChangeUserPositionCommand;
using Vpiska.Domain.Event.Interfaces;
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
            var storage = socketContext.ServiceProvider.GetRequiredService<IUserConnectionsStorage>();
            storage.AddConnection(socketContext.ConnectionId);
            return Task.CompletedTask;
        }

        public Task OnDisconnect(WebSocketContext socketContext)
        {
            var storage = socketContext.ServiceProvider.GetRequiredService<IUserConnectionsStorage>();
            
            if (!storage.RemoveConnection(socketContext.ConnectionId))
            {
                var logger = socketContext.ServiceProvider.GetRequiredService<ILogger<RangeListener>>();
                logger.LogWarning("Can't remove user connection {}", socketContext.ConnectionId);
            }
            
            return Task.CompletedTask;
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