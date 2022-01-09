using System.Threading.Tasks;
using Orleans;
using Vpiska.Api.Models.Event;

namespace Vpiska.Api.Orleans
{
    public sealed class EventGrain : Grain<Event>, IEventGrain
    {
        public Task SetData(Event data)
        {
            State = data;
            return Task.CompletedTask;
        }

        public Task<Event> GetData() => State.Id == null
            ? Task.FromResult<Event>(null)
            : Task.FromResult(State);
    }
}