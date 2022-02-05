using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Infrastructure.Orleans
{
    internal sealed class EventStateManager : IEventStateManager
    {
        private readonly IClusterClient _clusterClient;

        public EventStateManager(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public Task AddChatMessage(string eventId, ChatMessage chatMessage)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.AddChatMessage(chatMessage);
        }

        public Task<ChatMessage[]> GetChatMessages(string eventId)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.GetChatMessages();
        }

        public Task<bool> AddUserInfo(string eventId, UserInfo userInfo)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.AddUserInfo(userInfo);
        }

        public Task<bool> RemoveUserInfo(string eventId, string userId)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.RemoveUserInfo(userId);
        }

        public Task<bool> AddMediaLink(string eventId, string mediaLink)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.AddMediaLink(mediaLink);
        }

        public Task<bool> RemoveMediaLink(string eventId, string mediaLink)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.RemoveMediaLink(mediaLink);
        }

        public Task<bool> UpdateLocation(string eventId, string address, Coordinates coordinates)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.UpdateData(address, coordinates);
        }
    }
}