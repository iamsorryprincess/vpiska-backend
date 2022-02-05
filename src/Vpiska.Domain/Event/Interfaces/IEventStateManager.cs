using System.Threading.Tasks;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventStateManager
    {
        Task AddChatMessage(string eventId, ChatMessage chatMessage);
        
        Task<ChatMessage[]> GetChatMessages(string eventId);

        Task<bool> AddUserInfo(string eventId, UserInfo userInfo);

        Task<bool> RemoveUserInfo(string eventId, string userId);

        Task<bool> AddMediaLink(string eventId, string mediaLink);

        Task<bool> RemoveMediaLink(string eventId, string mediaLink);

        Task<bool> UpdateLocation(string eventId, string address, Coordinates coordinates);
    }
}