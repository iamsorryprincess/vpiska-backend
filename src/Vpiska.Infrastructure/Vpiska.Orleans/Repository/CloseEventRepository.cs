using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class CloseEventRepository : ICloseEventRepository
    {
        private readonly IClusterClient _clusterClient;

        public CloseEventRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public async Task<bool> CheckOwnership(Guid eventId, Guid ownerId)
        {
            var eventGrain = _clusterClient.GetGrain<IEventGrain>(eventId);
            var eventOwnerId = await eventGrain.GetOwnerId();

            if (eventOwnerId.HasValue)
            {
                return eventOwnerId.Value == ownerId;
            }

            return false;
        }

        public async Task<bool> RemoveEvent(Guid eventId)
        {
            var eventGrain = _clusterClient.GetGrain<IEventGrain>(eventId);
            var @event = await eventGrain.GetEvent();

            if (@event == null)
            {
                return false;
            }
            
            var areaGrain = await eventGrain.GetCurrentArea();
            var isSuccess = await areaGrain.RemoveEvent(eventGrain);

            if (isSuccess)
            {
                await eventGrain.Deactivate();
            }
            
            return isSuccess;
        }
    }
}