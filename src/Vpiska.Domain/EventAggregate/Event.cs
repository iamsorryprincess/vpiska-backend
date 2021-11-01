using System;
using System.Collections.Generic;
using System.Linq;

namespace Vpiska.Domain.EventAggregate
{
    public sealed class Event
    {
        private readonly List<UserInfo> _users;
        private readonly List<ChatMessage> _chatData;
        private readonly List<string> _mediaLinks;

        public Guid Id { get; }

        public Guid OwnerId { get; }

        public string Name { get; }

        public string Coordinates { get; }

        public string Address { get; }
        
        public IReadOnlyList<UserInfo> Users => _users;

        public IReadOnlyList<ChatMessage> ChatData => _chatData;

        public IReadOnlyList<string> MediaLinks => _mediaLinks;

        public Event(Guid id, Guid ownerId,
            string name, string coordinates, string address)
        {
            Id = id;
            OwnerId = ownerId;
            Name = name;
            Coordinates = coordinates;
            Address = address;
            _users = new List<UserInfo>();
            _chatData = new List<ChatMessage>();
            _mediaLinks = new List<string>();
        }

        public bool TryAddUser(Guid id, string name, string imageId)
        {
            if (_users.Any(x => x.Id == id))
            {
                return false;
            }
            
            _users.Add(new UserInfo(id, name, imageId));
            return true;
        }

        public bool TryRemoveUser(Guid id, out UserInfo user)
        {
            user = _users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                return false;
            }

            _users.Remove(user);
            return true;
        }

        public void AddChatMessage(Guid userId, string message) => _chatData.Add(new ChatMessage(userId, message));

        public bool TryAddMedia(string mediaId)
        {
            if (_mediaLinks.Any(id => id == mediaId))
            {
                return false;
            }
            
            _mediaLinks.Add(mediaId);
            return true;
        }

        public bool TryRemoveMedia(string mediaId)
        {
            var mediaLink = _mediaLinks.FirstOrDefault(id => id == mediaId);

            if (mediaLink == null)
            {
                return false;
            }

            _mediaLinks.Remove(mediaLink);
            return true;
        }
    }
}