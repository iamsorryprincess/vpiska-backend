using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Events.UserConnectedEvent
{
    internal sealed class UserConnectedHandler : UsersCountUpdatedHandler<UserConnectedEvent>
    {
        public UserConnectedHandler(ICache<Event> cache, IEventRepository repository, IConnectionsStorage storage, IEventSender eventSender)
            : base(cache, repository, storage, eventSender)
        {
        }
    }
}