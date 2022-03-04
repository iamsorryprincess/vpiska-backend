using System.Collections.Generic;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Events.EventCreatedEvent
{
    public sealed class EventCreatedEvent : IDomainEvent
    {
        public string EventId { get; set; }
        
        public string OwnerId { get; set; }

        public string Name { get; set; }
        
        public string Address { get; set; }
        
        public Coordinates Coordinates { get; set; }

        public List<string> MediaLinks { get; set; } = new();

        public List<ChatMessage> ChatData { get; set; } = new();

        public List<UserInfo> Users { get; set; } = new();

        public EventShortResponse ToShortResponse() => new()
        {
            Id = EventId,
            Name = Name,
            Coordinates = Coordinates,
            UsersCount = Users.Count
        };

        public Event ToModel() => new()
        {
            Id = EventId,
            OwnerId = OwnerId,
            Name = Name,
            Address = Address,
            Coordinates = Coordinates,
            ChatData = ChatData,
            Users = Users,
            MediaLinks = MediaLinks
        };

        public static EventCreatedEvent FromModel(Event model) => new()
        {
            EventId = model.Id,
            OwnerId = model.OwnerId,
            Name = model.Name,
            Address = model.Address,
            Coordinates = model.Coordinates,
            MediaLinks = model.MediaLinks,
            ChatData = model.ChatData,
            Users = model.Users
        };
    }
}