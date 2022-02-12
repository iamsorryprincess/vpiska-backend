using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Infrastructure.EventState
{
    internal sealed class EventState : IEventState
    {
        private readonly ConcurrentDictionary<string, Event> _events;
        private readonly ConcurrentDictionary<string, object> _userLockers;
        private readonly ConcurrentDictionary<string, object> _mediaLockers;
        private readonly ConcurrentDictionary<string, object> _updateLockers;
        private readonly ConcurrentDictionary<string, object> _chatLockers;

        public EventState()
        {
            _events = new ConcurrentDictionary<string, Event>();
            _userLockers = new ConcurrentDictionary<string, object>();
            _mediaLockers = new ConcurrentDictionary<string, object>();
            _updateLockers = new ConcurrentDictionary<string, object>();
            _chatLockers = new ConcurrentDictionary<string, object>();
        }

        public Task<List<EventShortResponse>> GetDataByRange(double xLeft, double xRight, double yBottom, double yTop) =>
            Task.FromResult(_events
                .Where(x => x.Value.Coordinates.X >= xLeft && x.Value.Coordinates.X <= xRight &&
                            x.Value.Coordinates.Y >= yBottom && x.Value.Coordinates.Y <= yTop)
                .Select(x => EventShortResponse.FromModel(x.Value))
                .ToList());

        public Task<Event> GetData(string eventId) => _events.TryGetValue(eventId, out var value)
            ? Task.FromResult(value)
            : Task.FromResult<Event>(null);

        public Task SetData(Event data)
        {
            _events.TryAdd(data.Id, data);
            _userLockers.TryAdd(data.Id, new object());
            _mediaLockers.TryAdd(data.Id, new object());
            _updateLockers.TryAdd(data.Id, new object());
            _chatLockers.TryAdd(data.Id, new object());
            return Task.CompletedTask;
        }

        public Task<bool> RemoveData(string eventId)
        {
            if (!_events.TryRemove(eventId, out _))
            {
                return Task.FromResult(false);
            }

            _userLockers.TryRemove(eventId, out _);
            _mediaLockers.TryRemove(eventId, out _);
            _updateLockers.TryRemove(eventId, out _);
            _chatLockers.TryRemove(eventId, out _);
            return Task.FromResult(true);
        }

        public Task<bool> AddUserInfo(string eventId, UserInfo userInfo)
        {
            if (!_events.TryGetValue(eventId, out var model))
            {
                return Task.FromResult(false);
            }
            
            if (model.Users.Any(x => x.UserId == userInfo.UserId))
            {
                return Task.FromResult(false);
            }

            lock (_userLockers[eventId])
            {
                model.Users.Add(userInfo);
            }
            
            return Task.FromResult(true);
        }

        public Task<bool> RemoveUserInfo(string eventId, string userId)
        {
            if (!_events.TryGetValue(eventId, out var model))
            {
                return Task.FromResult(false);
            }

            var user = model.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
            {
                return Task.FromResult(false);
            }

            lock (_userLockers[eventId])
            {
                model.Users.Remove(user);
            }
            
            return Task.FromResult(true);
        }

        public Task<bool> AddMediaLink(string eventId, string mediaLink)
        {
            if (!_events.TryGetValue(eventId, out var model))
            {
                return Task.FromResult(false);
            }
            
            if (model.MediaLinks.Contains(mediaLink))
            {
                return Task.FromResult(false);
            }

            lock (_mediaLockers[eventId])
            {
                model.MediaLinks.Add(mediaLink);
            }
            
            return Task.FromResult(true);
        }

        public Task<bool> RemoveMediaLink(string eventId, string mediaLink)
        {
            if (!_events.TryGetValue(eventId, out var model))
            {
                return Task.FromResult(false);
            }

            if (!model.MediaLinks.Contains(mediaLink))
            {
                return Task.FromResult(false);
            }

            lock (_mediaLockers[eventId])
            {
                model.MediaLinks.Remove(mediaLink);
            }
            
            return Task.FromResult(true);
        }

        public Task<bool> UpdateLocation(string eventId, string address, Coordinates coordinates)
        {
            if (!_events.TryGetValue(eventId, out var model))
            {
                return Task.FromResult(false);
            }

            lock (_updateLockers[eventId])
            {
                model.Address = address;
                model.Coordinates = coordinates;
            }
            
            return Task.FromResult(true);
        }

        public Task<bool> AddChatMessage(string eventId, ChatMessage chatMessage)
        {
            if (!_events.TryGetValue(eventId, out var model))
            {
                return Task.FromResult(false);
            }

            lock (_chatLockers[eventId])
            {
                model.ChatData.Add(chatMessage);
            }

            return Task.FromResult(true);
        }
    }
}