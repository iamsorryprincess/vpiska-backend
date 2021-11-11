using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Serilog;
using Vpiska.WebSocket;

namespace Vpiska.ApiChat.Connectors
{
    public sealed class ChatConnector : IWebSocketConnector
    {
        private const string ParamName = "eventId";

        private readonly ILogger _logger;
        private readonly UserConnectionsStorage _storage;

        public ChatConnector(ILogger logger, UserConnectionsStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }
        
        public void OnConnect(Guid connectionId, WebSocketUserContext userContext, Dictionary<string, string> queryParams)
        {
            if (Guid.TryParse(queryParams[ParamName], out var eventId))
            {
                if (!_storage.IsEventExist(eventId))
                {
                    if (_storage.TryCreateUserGroup(eventId))
                    {
                        //todo subscribe to event
                    }
                }

                if (!_storage.TryAddUserContext(eventId, connectionId, userContext))
                {
                    _logger.Warning("can't add user context. EventId: {eventId}. ConnectionId: {connectionId}", eventId,
                        connectionId);
                }
            }
        }

        public void OnDisconnect(Guid connectionId, Dictionary<string, string> queryParams)
        {
            if (Guid.TryParse(queryParams[ParamName], out var eventId))
            {
                if (_storage.TryRemoveUserContext(eventId, connectionId))
                {
                    if (_storage.GetUsersCount(eventId) == 0)
                    {
                        if (_storage.TryRemoveUserGroup(eventId))
                        {
                            //todo unsubscribe from event
                        }
                    }
                }
            }
        }
    }

    public sealed class UserConnectionsStorage
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, WebSocketUserContext>>
            _eventConnections;
        private readonly ConcurrentDictionary<Guid, Guid> _userConnections;

        public UserConnectionsStorage()
        {
            _eventConnections = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, WebSocketUserContext>>();
            _userConnections = new ConcurrentDictionary<Guid, Guid>();
        }

        public bool IsEventExist(Guid eventId) => _eventConnections.ContainsKey(eventId);

        public int GetUsersCount(Guid eventId) =>
            _eventConnections.TryGetValue(eventId, out var users) ? users.Count : 0;

        public bool TryCreateUserGroup(Guid eventId) =>
            _eventConnections.TryAdd(eventId, new ConcurrentDictionary<Guid, WebSocketUserContext>());

        public bool TryRemoveUserGroup(Guid eventId) =>
            _eventConnections.TryRemove(eventId, out _);

        public bool TryAddUserContext(Guid eventId, Guid connectionId, WebSocketUserContext context) =>
            _eventConnections.TryGetValue(eventId, out var users)
            && users.TryAdd(connectionId, context)
            && _userConnections.TryAdd(context.UserId, connectionId);

        public bool TryRemoveUserContext(Guid eventId, Guid connectionId) =>
            _eventConnections.TryGetValue(eventId, out var users)
            && users.TryRemove(connectionId, out var user)
            && _userConnections.TryRemove(user.UserId, out _);
    }
}