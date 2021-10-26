using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class GetByIdRepository : IGetByIdRepository
    {
        private readonly IClusterClient _clusterClient;

        public GetByIdRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task<Event> GetById(Guid eventId)
        {
            var eventGrain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return eventGrain.GetEvent();
        }
    }
}