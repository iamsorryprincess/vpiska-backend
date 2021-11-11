using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate;

namespace Vpiska.Orleans.Interfaces
{
    internal interface IEventGrain : IGrainWithGuidKey
    {
        Task<IAreaGrain> GetCurrentArea();

        Task SetCurrentArea(IAreaGrain areaGrain);

        Task Deactivate();

        Task<Event> GetEvent();

        Task SetEvent(Event @event);

        Task<Guid?> GetOwnerId();

        Task<bool> AddUserInfo(Guid userId, string name, string imageId);
        
        Task<bool> RemoveUserInfo(Guid userId);

        Task<bool> AddChatData(Guid userId, string message);

        Task<bool> AddMediaLink(string link);
        
        Task<bool> RemoveMediaLink(string link);
    }
}