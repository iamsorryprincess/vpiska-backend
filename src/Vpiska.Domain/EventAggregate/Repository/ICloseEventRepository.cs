using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface ICloseEventRepository
    {
        Task<bool> CheckOwnership(Guid eventId, Guid ownerId);

        Task<bool> RemoveEvent(Guid eventId);
    }
}