using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Responses;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<List<EventShortResponse>> GetByRange(
            double xLeft,
            double xRight,
            double yLeft,
            double yRight,
            CancellationToken cancellationToken = default);

        Task<bool> AddMediaLink(string eventId, string mediaLink, CancellationToken cancellationToken = default);

        Task<bool> RemoveMediaLink(string eventId, string mediaLink, CancellationToken cancellationToken = default);
    }
}