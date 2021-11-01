using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface IRemoveMediaRepository
    {
        Task<bool> RemoveMedia(Guid eventId, string mediaLink);
    }
}