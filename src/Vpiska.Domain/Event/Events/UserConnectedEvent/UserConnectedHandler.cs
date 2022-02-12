using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.UserConnectedEvent
{
    internal sealed class UserConnectedHandler : UsersCountUpdatedHandler<UserConnectedEvent>
    {
        private readonly IEventState _eventState;
        
        public UserConnectedHandler(IEventState eventState,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender) : base(eventState, repository, eventConnectionsStorage, eventSender, userConnectionsStorage, userSender)
        {
            _eventState = eventState;
        }

        public override async Task Handle(UserConnectedEvent domainEvent)
        {
            await _eventState.AddUserInfo(domainEvent.EventId, domainEvent.UserInfo);
            await base.Handle(domainEvent);
        }
    }
}