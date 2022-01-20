using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Commands.AddUserCommand;
using Vpiska.Domain.Event.Commands.RemoveUserCommand;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Interfaces;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class ChatConnector : IWebSocketConnector
    {
        private const string EventId = "eventId";
        private const string UserId = "Id";
        
        private readonly ICommandHandler<AddUserCommand> _addUserCommandHandler;
        private readonly ICommandHandler<RemoveUserCommand> _removeUserCommandHandler;

        public ChatConnector(ICommandHandler<AddUserCommand> addUserCommandHandler,
            ICommandHandler<RemoveUserCommand> removeUserCommandHandler)
        {
            _addUserCommandHandler = addUserCommandHandler;
            _removeUserCommandHandler = removeUserCommandHandler;
        }

        public Task OnConnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            var command = new AddUserCommand()
            {
                ConnectionId = connectionId, EventId = queryParams[EventId],
                UserInfo = new UserInfo() { UserId = identityParams[UserId] }
            };
            return _addUserCommandHandler.HandleAsync(command);
        }

        public Task OnDisconnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            var command = new RemoveUserCommand()
            {
                ConnectionId = connectionId,
                EventId = queryParams[EventId],
                UserId = identityParams[UserId]
            };
            return _removeUserCommandHandler.HandleAsync(command);
        }
    }
}