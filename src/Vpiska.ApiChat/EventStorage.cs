using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vpiska.WebSocket;

namespace Vpiska.ApiChat
{
    public sealed class EventStorage
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, WebSocketUserContext>> _events;

        public EventStorage()
        {
            _events = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, WebSocketUserContext>>();
        }

        public bool IsEventExist(Guid eventId) => _events.ContainsKey(eventId);

        public bool TryAddEvent(Guid eventId) => _events.TryAdd(eventId, new ConcurrentDictionary<Guid, WebSocketUserContext>());

        public bool TryAddUser(Guid eventId, Guid connectionId, WebSocketUserContext user) =>
            _events.TryGetValue(eventId, out var users) && users.TryAdd(connectionId, user);

        public bool TryRemoveEvent(Guid eventId) => _events.TryRemove(eventId, out _);

        public bool TryRemoveUser(Guid eventId, Guid connectionId) =>
            _events.TryGetValue(eventId, out var users) && users.TryRemove(connectionId, out _);

        public bool TryGetUsersByEventId(Guid eventId, out ICollection<Guid> connectionIds)
        {
            if (_events.TryGetValue(eventId, out var usersCollection))
            {
                // todo рассмотреть еще варианты выборки списка ключей т.к Keys алоцирует новый список и захватывает все локи
                connectionIds = usersCollection.Keys;
                return true;
            }

            connectionIds = null;
            return false;
        }
    }
}