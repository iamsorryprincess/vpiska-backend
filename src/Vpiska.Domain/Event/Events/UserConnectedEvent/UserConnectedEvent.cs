using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Events.UserConnectedEvent
{
    public sealed class UserConnectedEvent : IDomainEvent
    {
        public string EventId { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}