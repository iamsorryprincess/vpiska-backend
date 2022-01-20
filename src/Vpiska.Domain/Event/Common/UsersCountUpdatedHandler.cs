using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Common
{
    internal abstract class UsersCountUpdatedHandler<TEvent> : IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        private readonly ICache<Event> _cache;
        private readonly IEventRepository _repository;
        private readonly IConnectionsStorage _storage;
        private readonly IEventSender _eventSender;

        protected UsersCountUpdatedHandler(ICache<Event> cache,
            IEventRepository repository,
            IConnectionsStorage storage,
            IEventSender eventSender)
        {
            _cache = cache;
            _repository = repository;
            _storage = storage;
            _eventSender = eventSender;
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

            if (_storage.IsEventGroupExist(domainEvent.EventId))
            {
                var connections = _storage.GetConnections(domainEvent.EventId);

                if (connections.Any())
                {
                    await _eventSender.SendUsersCountUpdate(connections, model.Users.Count);
                }
            }
        }
    }
}