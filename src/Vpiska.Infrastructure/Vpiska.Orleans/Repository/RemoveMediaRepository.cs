using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class RemoveMediaRepository : IRemoveMediaRepository
    {
        private readonly IClusterClient _clusterClient;

        public RemoveMediaRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task<bool> RemoveMedia(Guid eventId, string mediaLink)
        {
            var eventGrain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return eventGrain.RemoveMediaLink(mediaLink);
        }
    }
}