using Vpiska.Domain.Base;
using Vpiska.Domain.Models;

namespace Vpiska.Domain.Events
{
    public sealed class UserLoggedIn : ChatEvent
    {
        public UserInfo User { get; set; }

        public UserInfo[] OtherUsers { get; set; }
    }
}