using System.Threading.Tasks;
using Orleans;

namespace Vpiska.Infrastructure.Orleans.Grains.Interfaces
{
    public interface IAreaGrain : IGrainWithStringKey
    {
        Task<bool> AddEvent(IEventGrain eventGrain);

        Task<bool> RemoveEvent(string eventId);

        Task<IEventGrain[]> GetEventGrains();
        
        Task<bool> CheckOwnerId(string ownerId);
    }
}