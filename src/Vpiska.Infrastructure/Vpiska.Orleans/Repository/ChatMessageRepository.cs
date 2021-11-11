using System;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Interfaces;

namespace Vpiska.Orleans.Repository
{
    internal sealed class ChatMessageRepository : IChatMessageRepository
    {
        private readonly IClusterClient _clusterClient;

        public ChatMessageRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task<bool> AddChatMessage(Guid eventId, Guid userId, string message)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.AddChatData(userId, message);
        }
    }
}