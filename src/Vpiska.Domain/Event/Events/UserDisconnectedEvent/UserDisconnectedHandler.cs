using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Events.UserDisconnectedEvent
{
    internal sealed class UserDisconnectedHandler : UsersCountUpdatedHandler<UserDisconnectedEvent>
    {
        public UserDisconnectedHandler(ICache<Event> cache, IEventRepository repository, IConnectionsStorage storage, IEventSender eventSender)
            : base(cache, repository, storage, eventSender)
        {
        }
    }
}