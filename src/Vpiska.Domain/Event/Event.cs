using System.Collections.Generic;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event
{
    public sealed class Event
    {
        public string Id { get; }

        public string OwnerId { get; }

        public string Name { get; }

        public string Address { get; set; }

        public Coordinates Coordinates { get; set; }

        public List<string> MediaLinks { get; }

        public List<ChatMessage> ChatData { get; }

        public List<UserInfo> Users { get; }

        public Event(string id, string ownerId, string name, string address, Coordinates coordinates,
            List<string> mediaLinks,
            List<ChatMessage> chatData,
            List<UserInfo> users)
        {
            Id = id;
            OwnerId = ownerId;
            Name = name;
            Address = address;
            Coordinates = coordinates;
            MediaLinks = mediaLinks;
            ChatData = chatData;
            Users = users;
        }
    }
}