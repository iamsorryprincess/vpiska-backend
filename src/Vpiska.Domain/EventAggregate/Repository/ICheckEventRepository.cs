using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface ICheckEventRepository
    {
        Task<bool> IsEventExist(Guid eventId);
        
        Task<bool> CheckOwnership(Guid eventId, Guid ownerId);
    }
}