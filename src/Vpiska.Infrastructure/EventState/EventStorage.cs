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
    internal sealed class EventStorage : IEventStorage
    {
        private readonly ConcurrentDictionary<string, EventState> _events;

        public EventStorage()
        {
            _events = new ConcurrentDictionary<string, EventState>();
        }

        public Task<List<EventShortResponse>> GetDataByRange(double xLeft, double xRight, double yBottom, double yTop) =>
            Task.FromResult(_events
                .Where(x => x.Value.State.Coordinates.X >= xLeft &&
                            x.Value.State.Coordinates.X <= xRight &&
                            x.Value.State.Coordinates.Y >= yBottom &&
                            x.Value.State.Coordinates.Y <= yTop)
                .Select(x => EventShortResponse.FromModel(x.Value.State))
                .ToList());

        public Task<Event> GetData(string eventId) => _events.TryGetValue(eventId, out var value)
            ? Task.FromResult(value.State)
            : Task.FromResult<Event>(null);

        public Task SetData(Event data)
        {
            _events.TryAdd(data.Id, new EventState(data));
            return Task.CompletedTask;
        }

        public Task<bool> RemoveData(string eventId) => Task.FromResult(_events.TryRemove(eventId, out _));

        public Task<bool> AddUserInfo(string eventId, UserInfo userInfo) =>
            Task.FromResult(_events.TryGetValue(eventId, out var state) && state.AddUserInfo(userInfo));

        public Task<bool> RemoveUserInfo(string eventId, string userId) =>
            Task.FromResult(_events.TryGetValue(eventId, out var state) && state.RemoveUserInfo(userId));

        public Task<bool> AddMediaLink(string eventId, string mediaLink) =>
            Task.FromResult(_events.TryGetValue(eventId, out var state) && state.AddMediaLink(mediaLink));

        public Task<bool> RemoveMediaLink(string eventId, string mediaLink) =>
            Task.FromResult(_events.TryGetValue(eventId, out var state) && state.RemoveMediaLink(mediaLink));

        public Task<bool> UpdateLocation(string eventId, string address, Coordinates coordinates)
        {
            if (!_events.TryGetValue(eventId, out var state))
            {
                return Task.FromResult(false);
            }

            state.UpdateLocation(address, coordinates);
            return Task.FromResult(true);
        }

        public Task<bool> AddChatMessage(string eventId, ChatMessage chatMessage)
        {
            if (!_events.TryGetValue(eventId, out var state))
            {
                return Task.FromResult(false);
            }
            
            state.AddChatMessage(chatMessage);
            return Task.FromResult(true);
        }
    }
}