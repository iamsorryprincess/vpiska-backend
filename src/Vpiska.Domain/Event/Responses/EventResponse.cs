using System.Collections.Generic;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Responses
{
    public sealed class EventResponse
    {
        public string Id { get; set; }

        public string OwnerId { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int UsersCount { get; set; }

        public Coordinates Coordinates { get; set; }

        public List<MediaInfo> Media { get; set; } = new();

        public List<ChatMessage> ChatData { get; set; } = new();

        public static EventResponse FromModel(Event model) => new()
        {
            Id = model.Id,
            OwnerId = model.OwnerId,
            Name = model.Name,
            Address = model.Address,
            UsersCount = model.Users.Count,
            Coordinates = model.Coordinates,
            Media = model.Media,
            ChatData = model.ChatData
        };
    }
}