using System;
using System.Collections.Concurrent;
using System.Linq;
using Vpiska.Domain.Models;

namespace Vpiska.Chat
{
    public sealed class Storage
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, UserInfo>> _eventGroups;
        private readonly ConcurrentDictionary<string, Guid> _usersConnections;

        public Storage()
        {
            _eventGroups = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, UserInfo>>();
            _usersConnections = new ConcurrentDictionary<string, Guid>();
        }

        public bool Exist(string eventId) => _eventGroups.ContainsKey(eventId);

        public bool CreateUserGroup(string eventId) =>
            _eventGroups.TryAdd(eventId, new ConcurrentDictionary<Guid, UserInfo>());

        public bool RemoveUserGroup(string eventId) => _eventGroups.TryRemove(eventId, out _);

        public int GetUsersCount(string eventId) => _eventGroups.TryGetValue(eventId, out var users) ? users.Count : 0;

        public UserInfo GetUserInfo(string eventId, Guid connectionId)
        {
            if (_eventGroups.TryGetValue(eventId, out var users))
            {
                return users.TryGetValue(connectionId, out var user) ? user : null;
            }

            return null;
        }

        public bool AddUserInfo(string eventId, Guid connectionId, UserInfo userInfo) =>
            _eventGroups.TryGetValue(eventId, out var users) &&
            users.TryAdd(connectionId, userInfo) &&
            _usersConnections.TryAdd(userInfo.Id, connectionId);

        public bool RemoveUserInfo(string eventId, Guid connectionId)
        {
            if (_eventGroups.TryGetValue(eventId, out var users))
            {
                return users.TryRemove(connectionId, out var user) && _usersConnections.TryRemove(user.Id, out _);
            }

            return false;
        }

        public Guid GetUserConnectionId(string userId) =>
            _usersConnections.TryGetValue(userId, out var connectionId)
                ? connectionId
                : Guid.Empty;

        public Guid[] GetUsersConnections(string eventId) => _eventGroups.TryGetValue(eventId, out var users)
            ? users.Select(x => x.Key).ToArray()
            : null;

        public Guid[] GetUsersConnectionsExceptOne(string eventId, string userId)
        {
            if (_eventGroups.TryGetValue(eventId, out var users))
            {
                return users
                    .Where(x => x.Value.Id != userId)
                    .Select(x => x.Key)
                    .ToArray();
            }

            return null;
        }
    }
}