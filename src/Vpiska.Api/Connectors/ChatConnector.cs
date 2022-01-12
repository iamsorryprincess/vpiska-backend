using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Vpiska.Api.Common;
using Vpiska.Api.Orleans;
using Vpiska.WebSocket;

namespace Vpiska.Api.Connectors
{
    public sealed class ChatConnector : IWebSocketConnector
    {
        private const string EventId = "eventId";
        private const string UserId = "Id";

        private readonly ILogger<ChatConnector> _logger;
        private readonly Storage _storage;
        private readonly IClusterClient _clusterClient;

        public ChatConnector(ILogger<ChatConnector> logger,
            Storage storage,
            IClusterClient clusterClient)
        {
            _logger = logger;
            _storage = storage;
            _clusterClient = clusterClient;
        }
        
        public async Task OnConnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            var eventId = queryParams[EventId];

            if (!_storage.IsEventGroupExist(eventId))
            {
                if (!_storage.CreateEventGroup(eventId))
                {
                    _logger.LogWarning("Can't create event group for event: {0}", eventId);
                    return;
                }
            }

            var userId = identityParams[UserId];

            if (!_storage.AddConnection(eventId, connectionId, userId))
            {
                _logger.LogWarning("Can't add user connection. EventId: {0}, UserId: {1}", eventId, userId);
                return;
            }

            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            await grain.AddUser(userId);
        }

        public async Task OnDisconnect(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams)
        {
            var eventId = queryParams[EventId];
            var userId = identityParams[UserId];
            
            if (!_storage.RemoveConnection(eventId, connectionId))
            {
                _logger.LogWarning("Can't remove connection. EventId: {0}, UserId: {1}", eventId, userId);
                return;
            }

            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            await grain.RemoveUser(userId);
        }
    }
}