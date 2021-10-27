using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class MediaRepository : IMediaRepository
    {
        private readonly IClusterClient _clusterClient;

        public MediaRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task<bool> AddMedia(Guid eventId, string mediaLink)
        {
            var eventGrain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return eventGrain.AddMediaLink(mediaLink);
        }
    }
}