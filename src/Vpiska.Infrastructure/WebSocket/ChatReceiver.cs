using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vpiska.Domain.Event.Commands.ChatMessageCommand;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Interfaces;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class ChatReceiver : IWebSocketReceiver
    {
        private readonly ILogger<ChatReceiver> _logger;
        private readonly ICommandHandler<ChatMessageCommand> _commandHandler;

        public ChatReceiver(ILogger<ChatReceiver> logger, ICommandHandler<ChatMessageCommand> commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        public Task Receive(Guid connectionId, string route, string message,
            Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams)
        {
            switch (route)
            {
                case "chatMessage":
                {
                    var command = new ChatMessageCommand()
                    {
                        EventId = queryParams["eventId"],
                        ChatMessage = new ChatMessage()
                        {
                            UserId = identityParams["Id"],
                            UserName = identityParams["Name"],
                            UserImageId = identityParams["ImageId"],
                            Message = message
                        }
                    };
                    return _commandHandler.HandleAsync(command);
                }
                default:
                    _logger.LogWarning("Unknown receiver route {}", route);
                    return Task.CompletedTask;
            }
        }
    }
}