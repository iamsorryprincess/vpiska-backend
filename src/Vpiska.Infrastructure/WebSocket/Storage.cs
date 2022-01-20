using System;
using System.Collections.Concurrent;
using System.Linq;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class Storage : IConnectionsStorage
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, string>> _eventGroups;
        private readonly ConcurrentDictionary<string, Guid> _usersConnections;
        
        public Storage()
        {
            _eventGroups = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, string>>();
            _usersConnections = new ConcurrentDictionary<string, Guid>();
        }
        
        public bool IsEventGroupExist(string eventId) => _eventGroups.ContainsKey(eventId);

        public bool CreateEventGroup(string eventId) =>
            _eventGroups.TryAdd(eventId, new ConcurrentDictionary<Guid, string>());

        public bool RemoveEventGroup(string eventId)
        {
            if (_eventGroups.TryRemove(eventId, out var users))
            {
                foreach (var (_, userId) in users)
                {
                    _usersConnections.TryRemove(userId, out _);
                }
                users.Clear();
                return true;
            }

            return false;
        }

        public bool AddConnection(string eventId, Guid connectionId, string userId) =>
            _eventGroups.TryGetValue(eventId, out var users)
            && users.TryAdd(connectionId, userId)
            && _usersConnections.TryAdd(userId, connectionId);

        public bool RemoveConnection(string eventId, Guid connectionId) =>
            _eventGroups.TryGetValue(eventId, out var users)
            && users.TryRemove(connectionId, out var userId)
            && _usersConnections.TryRemove(userId, out _);

        public Guid[] GetConnections(string eventId)
        {
            return _eventGroups.TryGetValue(eventId, out var users)
                ? users.Select(x => x.Key).ToArray()
                : Array.Empty<Guid>();
        }

        public Guid GetUserConnectionId(string userId) =>
            _usersConnections.TryGetValue(userId, out var connectionId)
                ? connectionId
                : Guid.Empty;
    }
}