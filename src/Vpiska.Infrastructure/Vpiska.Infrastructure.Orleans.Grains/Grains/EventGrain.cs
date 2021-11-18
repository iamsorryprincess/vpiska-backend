using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event;
using Vpiska.Infrastructure.Orleans.Grains.Interfaces;

namespace Vpiska.Infrastructure.Orleans.Grains.Grains
{
    internal sealed class EventGrain : Grain, IEventGrain
    {
        private string _ownerId;
        private string _name;
        private string _coordinates;
        private string _address;
        private List<string> _mediaLinks;
        private List<ChatData> _chatData;
        private List<UserInfo> _users;

        private IAreaGrain _areaGrain;

        public Task SetData(Event @event, IAreaGrain areaGrain)
        {
            _ownerId = @event.OwnerId;
            _name = @event.Name;
            _coordinates = @event.Coordinates;
            _address = @event.Address;
            _mediaLinks = @event.MediaLinks.ToList();
            _chatData = @event.ChatData.ToList();
            _users = @event.Users.ToList();
            _areaGrain = areaGrain;
            return Task.CompletedTask;
        }

        public Task<Event> GetData() =>
            _name == null
                ? Task.FromResult<Event>(null)
                : Task.FromResult(new Event(this.GetPrimaryKeyString(), _ownerId, _name, _coordinates, _address, _mediaLinks.ToArray(),
                    _chatData.ToArray(), _users.ToArray()));

        public Task<string> GetOwnerId() => Task.FromResult(_ownerId);

        public Task<bool> TryAddMedia(string mediaLink)
        {
            if (_mediaLinks.Contains(mediaLink))
            {
                return Task.FromResult(false);
            }
            
            _mediaLinks.Add(mediaLink);
            return Task.FromResult(true);
        }

        public Task<bool> TryRemoveMedia(string mediaLink)
        {
            if (_mediaLinks.Contains(mediaLink))
            {
                _mediaLinks.Remove(mediaLink);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public Task<bool> TryAddUser(UserInfo userInfo)
        {
            if (_users.Any(x => x.Id == userInfo.Id))
            {
                return Task.FromResult(false);
            }
            
            _users.Add(userInfo);
            return Task.FromResult(true);
        }

        public Task<bool> TryRemoveUser(string userId)
        {
            var user = _users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return Task.FromResult(false);
            }

            _users.Remove(user);
            return Task.FromResult(true);
        }

        public Task AddChatData(ChatData chatData)
        {
            _chatData.Add(chatData);
            return Task.CompletedTask;
        }
    }
}