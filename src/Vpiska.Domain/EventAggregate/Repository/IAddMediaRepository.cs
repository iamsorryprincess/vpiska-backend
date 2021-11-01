using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Repository
{
    public interface IAddMediaRepository
    {
        Task<bool> AddMedia(Guid eventId, string mediaLink);
    }
}