using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Grains
{
    internal sealed class EventGrain : Grain, IEventGrain
    {
        private Event _event;
        private IAreaGrain _areaGrain;

        public Task<IAreaGrain> GetCurrentArea() => Task.FromResult(_areaGrain);
        
        public Task SetCurrentArea(IAreaGrain areaGrain)
        {
            _areaGrain = areaGrain;
            return Task.CompletedTask;
        }

        public Task Deactivate()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public Task<Event> GetEvent() => Task.FromResult(_event);

        public Task SetEvent(Event @event)
        {
            _event = @event;
            return Task.CompletedTask;
        }

        public Task<Guid?> GetOwnerId() => Task.FromResult(_event?.OwnerId);

        public Task<bool> AddUserInfo(Guid userId, string name, string imageId) =>
            Task.FromResult(_event.TryAddUser(userId, name, imageId));

        public Task<bool> RemoveUserInfo(Guid userId) => Task.FromResult(_event.TryRemoveUser(userId, out _));

        public Task<bool> AddMediaLink(string link) => Task.FromResult(_event.TryAddMedia(link));
    }
}