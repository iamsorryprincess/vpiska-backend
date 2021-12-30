using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Models;

namespace Vpiska.Mongo
{
    public interface IEventRepository : IMongoRepository<Event>
    {
        Task<bool> CheckForOwnershipAsync(string eventId, string ownerId, CancellationToken cancellationToken = default);

        Task<bool> AddMediaAsync(string eventId, string mediaId, CancellationToken cancellationToken = default);

        Task<bool> CheckMediaAsync(string eventId, string mediaId, CancellationToken cancellationToken = default);

        Task<bool> DeleteMediaAsync(string eventId, string mediaId, CancellationToken cancellationToken = default);
    }
}