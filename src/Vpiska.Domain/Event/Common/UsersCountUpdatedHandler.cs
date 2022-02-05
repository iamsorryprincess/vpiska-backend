using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Common
{
    internal abstract class UsersCountUpdatedHandler<TEvent> : IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        private readonly ICache<Event> _cache;
        private readonly IEventRepository _repository;
        private readonly IEventConnectionsStorage _eventConnectionsStorage;
        private readonly IEventSender _eventSender;
        private readonly IUserConnectionsStorage _userConnectionsStorage;
        private readonly IUserSender _userSender;

        protected UsersCountUpdatedHandler(ICache<Event> cache,
            IEventRepository repository,
            IEventConnectionsStorage eventConnectionsStorage,
            IEventSender eventSender,
            IUserConnectionsStorage userConnectionsStorage,
            IUserSender userSender)
        {
            _cache = cache;
            _repository = repository;
            _eventConnectionsStorage = eventConnectionsStorage;
            _eventSender = eventSender;
            _userConnectionsStorage = userConnectionsStorage;
            _userSender = userSender;
        }

        public async Task Handle(TEvent domainEvent)
        {
            var model = await _cache.GetData(domainEvent.EventId);

            if (model == null)
            {
                model = await _repository.GetByFieldAsync("_id", domainEvent.EventId);
                
                if (model == null)
                {
                    return;
                }

                await _cache.SetData(model);
            }

            if (_eventConnectionsStorage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _eventConnectionsStorage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await _eventSender.SendUsersCountUpdate(connections, model.Users.Count);
                }
            }

            var rangeConnections =
                _userConnectionsStorage.GetConnectionsByRange(model.Coordinates.X, model.Coordinates.Y);

            if (rangeConnections.Any())
            {
                await _userSender.SendEventUpdatedInfo(rangeConnections, EventUpdatedInfo.FromModel(model));
            }
        }
    }
}