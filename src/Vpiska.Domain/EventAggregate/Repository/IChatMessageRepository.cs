using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface IChatMessageRepository
    {
        Task<bool> AddChatMessage(Guid eventId, Guid userId, string message);
    }
}