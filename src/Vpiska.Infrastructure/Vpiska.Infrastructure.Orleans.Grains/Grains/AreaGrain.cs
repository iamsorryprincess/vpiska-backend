using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Infrastructure.Orleans.Grains.Interfaces;

namespace Vpiska.Infrastructure.Orleans.Grains.Grains
{
    internal sealed class AreaGrain : Grain, IAreaGrain
    {
        private readonly List<IEventGrain> _eventGrains;

        public AreaGrain()
        {
            _eventGrains = new List<IEventGrain>();
        }

        public Task<bool> AddEvent(IEventGrain eventGrain)
        {
            if (_eventGrains.Any(x => x.GetPrimaryKeyString() == eventGrain.GetPrimaryKeyString()))
            {
                return Task.FromResult(false);
            }
            
            _eventGrains.Add(eventGrain);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveEvent(string eventId)
        {
            var eventGrain = _eventGrains.FirstOrDefault(x => x.GetPrimaryKeyString() == eventId);

            if (eventGrain == null)
            {
                return Task.FromResult(false);
            }

            _eventGrains.Remove(eventGrain);
            return Task.FromResult(true);
        }

        public Task<IEventGrain[]> GetEventGrains() => Task.FromResult(_eventGrains.ToArray());

        public async Task<bool> CheckOwnerId(string ownerId)
        {
            var ownerIds = await Task.WhenAll(_eventGrains.Select(x => x.GetOwnerId()));
            return ownerIds.Contains(ownerId);
        }
    }
}