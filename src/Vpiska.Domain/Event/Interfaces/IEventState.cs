using System.Collections.Generic;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventState
    {
        Task<List<EventShortResponse>> GetDataByRange(double xLeft, double xRight, double yBottom, double yTop);
        
        Task<Event> GetData(string eventId);

        Task SetData(Event data);

        Task<bool> RemoveData(string eventId);

        Task<bool> AddUserInfo(string eventId, UserInfo userInfo);

        Task<bool> RemoveUserInfo(string eventId, string userId);

        Task<bool> AddMediaLink(string eventId, string mediaLink);

        Task<bool> RemoveMediaLink(string eventId, string mediaLink);

        Task<bool> UpdateLocation(string eventId, string address, Coordinates coordinates);

        Task<bool> AddChatMessage(string eventId, ChatMessage chatMessage);
    }
}