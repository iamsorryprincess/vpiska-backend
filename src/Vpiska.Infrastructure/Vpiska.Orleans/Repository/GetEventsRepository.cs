using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Domain.EventAggregate.Responses;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class GetEventsRepository : IGetEventsRepository
    {
        private readonly IClusterClient _clusterClient;

        public GetEventsRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public async Task<EventInfoResponse[]> GetEvents(string area)
        {
            var areaGrain = _clusterClient.GetGrain<IAreaGrain>(area);
            var eventGrains = await areaGrain.GetEventGrains();
            var events = await Task.WhenAll(eventGrains.Select(eventGrain => eventGrain.GetEvent()));

            return events
                .Where(x => x != null)
                .Select(@event => new EventInfoResponse()
                {
                    Id = @event.Id,
                    Name = @event.Name,
                    Coordinates = @event.Coordinates,
                    UsersCount = @event.Users.Count
                }).ToArray();
        }
    }
}