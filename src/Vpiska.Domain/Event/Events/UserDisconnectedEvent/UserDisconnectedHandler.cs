using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.UserDisconnectedEvent
{
    internal sealed class UserDisconnectedHandler : UsersCountUpdatedHandler<UserDisconnectedEvent>
    {
        private readonly IEventStorage _eventState;
        
        public UserDisconnectedHandler(IEventStorage eventState,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender) : base(eventState, repository, eventConnectionsStorage, eventSender, userConnectionsStorage, userSender)
        {
            _eventState = eventState;
        }

        public override async Task Handle(UserDisconnectedEvent domainEvent)
        {
            await _eventState.RemoveUserInfo(domainEvent.EventId, domainEvent.UserId);
            await base.Handle(domainEvent);
        }
    }
}