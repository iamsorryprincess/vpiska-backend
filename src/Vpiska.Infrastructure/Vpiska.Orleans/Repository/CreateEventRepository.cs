using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class CreateEventRepository : ICreateEventRepository
    {
        private readonly IClusterClient _clusterClient;

        public CreateEventRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task<bool> IsOwnerHasEvent(string areaName, Guid ownerId)
        {
            var areaGrain = _clusterClient.GetGrain<IAreaGrain>(areaName);
            return areaGrain.CheckOwnerId(ownerId);
        }

        public async Task<bool> Create(string area, Event @event)
        {
            var areaGrain = _clusterClient.GetGrain<IAreaGrain>(area);
            var eventGrain = _clusterClient.GetGrain<IEventGrain>(@event.Id);
            await eventGrain.SetEvent(@event);
            var result = await areaGrain.AddEvent(eventGrain);
            return result;
        }
    }
}