using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<List<Event>> GetAll(CancellationToken cancellationToken = default);
        
        Task<bool> AddMediaLink(string eventId, string mediaLink, CancellationToken cancellationToken = default);

        Task<bool> RemoveMediaLink(string eventId, string mediaLink, CancellationToken cancellationToken = default);
    }
}