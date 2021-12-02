using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event;

namespace Vpiska.Infrastructure.Orleans.Interfaces
{
    public interface IAreaGrain : IGrainWithStringKey
    {
        Task<bool> AddEvent(IEventGrain eventGrain);

        Task<bool> RemoveEvent(string eventId);

        Task<ShortEventResponse[]> GetShortEventsResponse();
        
        Task<bool> CheckOwnerId(string ownerId);
    }
}