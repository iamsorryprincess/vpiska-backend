using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.UserDisconnectedEvent
{
    internal sealed class UserDisconnectedHandler : UsersCountUpdatedHandler<UserDisconnectedEvent>
    {
        private readonly IEventStorage _eventStorage;
        
        public UserDisconnectedHandler(IEventStorage eventStorage,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender) : base(eventStorage, repository, eventConnectionsStorage, eventSender, userConnectionsStorage, userSender)
        {
            _eventStorage = eventStorage;
        }

        public override async Task Handle(UserDisconnectedEvent domainEvent)
        {
            await _eventStorage.RemoveUserInfo(domainEvent.EventId, domainEvent.UserId);
            await base.Handle(domainEvent);
        }
    }
}