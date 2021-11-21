using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Vpiska.Infrastructure.Websocket
{
    public sealed class UserConnectionsStorage
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, WebSocketUserContext>> _connections;
        private readonly ConcurrentDictionary<string, Guid> _userConnections;

        public UserConnectionsStorage()
        {
            _connections = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, WebSocketUserContext>>();
            _userConnections = new ConcurrentDictionary<string, Guid>();
        }

        public bool Exist(string eventId) => _connections.ContainsKey(eventId);

        public int GetUsersCount(string eventId) =>
            _connections.TryGetValue(eventId, out var users) ? users.Count : 0;

        public bool TryCreateUserGroup(string eventId) =>
            _connections.TryAdd(eventId, new ConcurrentDictionary<Guid, WebSocketUserContext>());
        
        public bool TryRemoveUserGroup(string eventId) =>
            _connections.TryRemove(eventId, out _);
        
        public bool TryAddUserContext(string eventId, Guid connectionId, WebSocketUserContext context) =>
            _connections.TryGetValue(eventId, out var users)
            && users.TryAdd(connectionId, context)
            && _userConnections.TryAdd(context.UserId, connectionId);

        public bool TryRemoveUserContext(string eventId, Guid connectionId, out WebSocketUserContext context)
        {
            context = null;
            return _connections.TryGetValue(eventId, out var users)
                   && users.TryRemove(connectionId, out context)
                   && _userConnections.TryRemove(context.UserId, out _);
        }

        public bool TryGetUserConnectionId(string userId, out Guid connectionId) =>
            _userConnections.TryGetValue(userId, out connectionId);

        public bool TryGetUsersConnectionsWithoutOne(string eventId, string userId, out Guid[] usersConnections)
        {
            usersConnections = null;
            
            if (_connections.TryGetValue(eventId, out var contexts))
            {
                usersConnections = contexts
                    .Where(x => x.Value.UserId != userId)
                    .Select(x => x.Key)
                    .ToArray();
                return true;
            }

            return false;
        }
        
        public bool TryGetUsersConnections(string eventId, out Guid[] usersConnections)
        {
            usersConnections = null;
            
            if (_connections.TryGetValue(eventId, out var contexts))
            {
                usersConnections = contexts
                    .Select(x => x.Key)
                    .ToArray();
                return true;
            }

            return false;
        }

        public bool TryGetUserInfo(string eventId, Guid connectionId, out WebSocketUserContext context)
        {
            context = null;
            return _connections.TryGetValue(eventId, out var users)
                   && users.TryGetValue(connectionId, out context);
        }
    }
}