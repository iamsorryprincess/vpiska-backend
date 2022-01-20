using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Infrastructure.Orleans
{
    internal sealed class EventsCache : ICache<Event>
    {
        private readonly IClusterClient _clusterClient;

        public EventsCache(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task<Event> GetData(string id)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(id);
            return grain.GetData();
        }

        public Task SetData(Event data)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(data.Id);
            return grain.Init(data);
        }

        public Task<bool> RemoveData(string id)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(id);
            return grain.Close();
        }
    }
}