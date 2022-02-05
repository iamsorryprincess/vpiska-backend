using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Infrastructure.Orleans
{
    internal interface IEventGrain : IGrainWithStringKey
    {
        Task<Event> GetData();

        Task Init(Event data);

        Task<bool> Close();

        Task AddChatMessage(ChatMessage chatMessage);

        Task<ChatMessage[]> GetChatMessages();

        Task<bool> AddUserInfo(UserInfo userInfo);

        Task<bool> RemoveUserInfo(string userId);

        Task<bool> AddMediaLink(string mediaLink);

        Task<bool> RemoveMediaLink(string mediaLink);

        Task<bool> UpdateData(string address, Coordinates coordinates);
    }
}