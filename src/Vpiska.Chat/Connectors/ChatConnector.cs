using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vpiska.Domain.Models;
using Vpiska.WebSocket;

namespace Vpiska.Chat.Connectors
{
    public sealed class ChatConnector : IWebSocketConnector
    {
        private const string EventId = "eventId";
        private const string UserId = "Id";
        private const string UserName = "Name";
        private const string UserImage = "ImageId";

        private readonly Storage _storage;

        public ChatConnector(Storage storage)
        {
            _storage = storage;
        }
        
        public Task OnConnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            var userInfo = new UserInfo()
            {
                Id = identityParams[UserId],
                Name = identityParams[UserName],
                ImageId = identityParams[UserImage]
            };

            if (_storage.Exist(queryParams[EventId]))
            {
                
            }
            
            return Task.CompletedTask;
        }

        public Task OnDisconnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            throw new NotImplementedException();
        }
    }
}