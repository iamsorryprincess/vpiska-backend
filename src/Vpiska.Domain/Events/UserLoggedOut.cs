using Vpiska.Domain.Base;

namespace Vpiska.Domain.Events
{
    public sealed class UserLoggedOut : ChatEvent
    {
        public string UserId { get; set; }
    }
}