using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Commands.AddUserCommand;
using Vpiska.Domain.Event.Commands.RemoveUserCommand;
using Vpiska.Domain.Event.Events.ChatMessageEvent;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Interfaces;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class ChatListener : IWebSocketListener
    {
        public Task OnConnect(WebSocketContext socketContext)
        {
            var commandHandler = socketContext.ServiceProvider.GetRequiredService<ICommandHandler<AddUserCommand>>();
            var command = new AddUserCommand()
            {
                ConnectionId = socketContext.ConnectionId,
                EventId = socketContext.QueryParams["eventId"],
                UserInfo = new UserInfo() { UserId = socketContext.IdentityParams["Id"] }
            };
            return commandHandler.HandleAsync(command);
        }

        public Task OnDisconnect(WebSocketContext socketContext)
        {
            var commandHandler = socketContext.ServiceProvider.GetRequiredService<ICommandHandler<RemoveUserCommand>>();
            var command = new RemoveUserCommand()
            {
                ConnectionId = socketContext.ConnectionId,
                EventId = socketContext.QueryParams["eventId"],
                UserId = socketContext.IdentityParams["Id"]
            };
            return commandHandler.HandleAsync(command);
        }

        public async Task Receive(WebSocketContext socketContext, string route, string message)
        {
            switch (route)
            {
                case "chatMessage":
                {
                    var eventBus = socketContext.ServiceProvider.GetRequiredService<IEventBus>();
                    var domainEvent = new ChatMessageEvent()
                    {
                        EventId = socketContext.QueryParams["eventId"],
                        ChatMessage = new ChatMessage()
                        {
                            UserId = socketContext.IdentityParams["Id"],
                            UserName = socketContext.IdentityParams["Name"],
                            UserImageId = socketContext.IdentityParams["ImageId"],
                            Message = message
                        }
                    };
                    await eventBus.PublishAsync(domainEvent);
                    return;
                }
                default:
                    var logger = socketContext.ServiceProvider.GetRequiredService<ILogger<ChatListener>>();
                    logger.LogWarning("Unknown receiver route {}", route);
                    return;
            }
        }
    }
}