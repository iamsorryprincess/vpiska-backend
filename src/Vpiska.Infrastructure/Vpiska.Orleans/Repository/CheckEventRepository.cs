using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class CheckEventRepository : ICheckEventRepository
    {
        private readonly IClusterClient _clusterClient;

        public CheckEventRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public async Task<bool> IsEventExist(Guid eventId)
        {
            var eventGrain = _clusterClient.GetGrain<IEventGrain>(eventId);
            var @event = await eventGrain.GetEvent();
            return @event != null;
        }
    }
}