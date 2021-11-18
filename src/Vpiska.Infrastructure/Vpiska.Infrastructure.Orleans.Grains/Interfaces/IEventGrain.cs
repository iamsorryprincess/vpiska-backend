using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event;

namespace Vpiska.Infrastructure.Orleans.Grains.Interfaces
{
    public interface IEventGrain : IGrainWithStringKey
    {
        Task SetData(Event @event, IAreaGrain areaGrain);

        Task<Event> GetData();

        Task<string> GetOwnerId();

        Task<bool> TryAddMedia(string mediaLink);

        Task<bool> TryRemoveMedia(string mediaLink);

        Task<bool> TryAddUser(UserInfo userInfo);

        Task<bool> TryRemoveUser(string userId);

        Task AddChatData(ChatData chatData);
    }
}