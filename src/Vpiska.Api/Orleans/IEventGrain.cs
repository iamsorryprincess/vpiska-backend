using System.Threading.Tasks;
using Orleans;
using Vpiska.Api.Models.Event;

namespace Vpiska.Api.Orleans
{
    public interface IEventGrain : IGrainWithStringKey
    {
        Task<Event> GetData();

        Task Init(Event data);

        Task Close();

        Task<bool> AddMedia(string mediaLink);

        Task<bool> RemoveMedia(string mediaLink);

        Task<bool> AddUser(string userId);

        Task<bool> RemoveUser(string userId);

        Task AddChatMessage(ChatMessage chatMessage);
    }
}