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

        public Task Init(Event data)
        {
            State = data;
            return Task.CompletedTask;
        }

        public Task Close()
        {
            DeactivateOnIdle();
            return ClearStateAsync();
        }

        public Task<bool> AddMedia(string mediaLink)
        {
            if (State.Id == null)
            {
                return Task.FromResult(false);
            }

            if (State.MediaLinks.Contains(mediaLink))
            {
                return Task.FromResult(false);
            }
            
            State.MediaLinks.Add(mediaLink);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveMedia(string mediaLink) =>
            Task.FromResult(State.Id != null && State.MediaLinks.Remove(mediaLink));
    }
}