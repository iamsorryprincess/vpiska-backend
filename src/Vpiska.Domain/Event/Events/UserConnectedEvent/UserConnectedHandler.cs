using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Events.UserConnectedEvent
{
    internal sealed class UserConnectedHandler : UsersCountUpdatedHandler<UserConnectedEvent>
    {
        public UserConnectedHandler(ICache<Event> cache,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender) : base(cache, repository, eventConnectionsStorage, eventSender, userConnectionsStorage, userSender)
        {
        }
    }
}