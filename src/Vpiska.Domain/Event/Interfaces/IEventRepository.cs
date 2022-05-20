using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<List<Event>> GetAll(CancellationToken cancellationToken = default);
        
        Task<bool> AddMediaLink(string eventId, MediaInfo mediaInfo, CancellationToken cancellationToken = default);

        Task<bool> RemoveMediaLink(string eventId, string mediaId, CancellationToken cancellationToken = default);
    }
}