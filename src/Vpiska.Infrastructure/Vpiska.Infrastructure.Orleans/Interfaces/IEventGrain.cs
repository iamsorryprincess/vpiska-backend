using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event;

namespace Vpiska.Infrastructure.Orleans.Interfaces
{
    public interface IEventGrain : IGrainWithStringKey
    {
        Task<bool> CheckData();
        
        Task SetData(Event @event, IAreaGrain areaGrain);

        Task<Event> GetData();

        Task<bool> Close();

        Task<string> GetOwnerId();

        Task<bool> CheckOwnership(string ownerId);

        Task<UserInfo[]> GetUsers();

        Task<bool> TryAddMedia(string mediaLink);

        Task<bool> TryRemoveMedia(string mediaLink);

        Task<bool> TryAddUser(UserInfo userInfo);

        Task<bool> TryRemoveUser(string userId);

        Task<bool> AddChatData(ChatData chatData);
    }
}