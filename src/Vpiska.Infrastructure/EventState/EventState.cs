using System.Linq;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Infrastructure.EventState
{
    internal sealed class EventState
    {
        public readonly Event State;

        private readonly object _userLocker;
        private readonly object _mediaLocker;
        private readonly object _updateLocker;
        private readonly object _chatLocker;

        public EventState(Event @event)
        {
            State = @event;
            _userLocker = new object();
            _mediaLocker = new object();
            _updateLocker = new object();
            _chatLocker = new object();
        }

        public bool AddUserInfo(UserInfo userInfo)
        {
            lock (_userLocker)
            {
                if (State.Users.Any(x => x.UserId == userInfo.UserId))
                {
                    return false;
                }
                
                State.Users.Add(userInfo);
            }

            return true;
        }

        public bool RemoveUserInfo(string userId)
        {
            lock (_userLocker)
            {
                var user = State.Users.FirstOrDefault(x => x.UserId == userId);
                return user != null && State.Users.Remove(user);
            }
        }

        public bool AddMediaLink(MediaInfo mediaInfo)
        {
            lock (_mediaLocker)
            {
                if (State.Media.All(x => x.Id != mediaInfo.Id))
                {
                    return false;
                }
                
                State.Media.Add(mediaInfo);
            }

            return true;
        }

        public bool RemoveMediaLink(string mediaId)
        {
            lock (_mediaLocker)
            {
                var mediaInfo = State.Media.FirstOrDefault(x => x.Id == mediaId);
                return mediaInfo != null && State.Media.Remove(mediaInfo);
            }
        }

        public void UpdateLocation(string address, Coordinates coordinates)
        {
            lock (_updateLocker)
            {
                State.Address = address;
                State.Coordinates = coordinates;
            }
        }

        public void AddChatMessage(ChatMessage chatMessage)
        {
            lock (_chatLocker)
            {
                State.ChatData.Add(chatMessage);
            }
        }
    }
}