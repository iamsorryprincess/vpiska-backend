using System;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Commands.AddUserCommand
{
    public sealed class AddUserCommand
    {
        public string EventId { get; set; }

        public Guid ConnectionId { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}