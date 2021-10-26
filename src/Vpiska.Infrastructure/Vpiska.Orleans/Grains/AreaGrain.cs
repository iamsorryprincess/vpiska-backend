using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Grains
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
            if (_eventGrains.Contains(eventGrain))
            {
                return Task.FromResult(false);
            }

            eventGrain.SetCurrentArea(this);
            _eventGrains.Add(eventGrain);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveEvent(IEventGrain eventGrain)
        {
            if (_eventGrains.Contains(eventGrain))
            {
                _eventGrains.Remove(eventGrain);
                eventGrain.SetCurrentArea(null);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public Task<IReadOnlyList<IEventGrain>> GetEventGrains() =>
            Task.FromResult<IReadOnlyList<IEventGrain>>(_eventGrains);

        public async Task<bool> CheckOwnerId(Guid ownerId)
        {
            var owners = await Task.WhenAll(_eventGrains.Select(x => x.GetOwnerId()));
            return owners.Where(x => x.HasValue).Contains(ownerId);
        }
    }
}