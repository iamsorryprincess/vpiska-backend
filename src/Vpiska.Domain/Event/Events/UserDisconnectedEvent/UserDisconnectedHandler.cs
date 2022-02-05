using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Events.UserDisconnectedEvent
{
    internal sealed class UserDisconnectedHandler : UsersCountUpdatedHandler<UserDisconnectedEvent>
    {
        public UserDisconnectedHandler(ICache<Event> cache,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender) : base(cache, repository, eventConnectionsStorage, eventSender, userConnectionsStorage, userSender)
        {
        }
    }
}