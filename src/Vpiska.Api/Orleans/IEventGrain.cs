using System.Threading.Tasks;
using Orleans;
using Vpiska.Api.Models.Event;

namespace Vpiska.Api.Orleans
{
    public interface IEventGrain : IGrainWithStringKey
    {
        Task SetData(Event data);

        Task<Event> GetData();
    }
}