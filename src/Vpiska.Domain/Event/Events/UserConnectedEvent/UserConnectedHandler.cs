using System.Threading.Tasks;
using Vpiska.Domain.Event.Common;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Domain.Event.Events.UserConnectedEvent
{
    internal sealed class UserConnectedHandler : UsersCountUpdatedHandler<UserConnectedEvent>
    {
        private readonly IEventStorage _eventStorage;
        
        public UserConnectedHandler(IEventStorage eventStorage,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender) : base(eventStorage, repository, eventConnectionsStorage, eventSender, userConnectionsStorage, userSender)
        {
            _eventStorage = eventStorage;
        }

        public override async Task Handle(UserConnectedEvent domainEvent)
        {
            await _eventStorage.AddUserInfo(domainEvent.EventId, domainEvent.UserInfo);
            await base.Handle(domainEvent);
        }
    }
}