using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Media.Interfaces
{
    public interface IMediaRepository : IRepository<Media>
    {
        Task<bool> UpdateAsync(Media media, CancellationToken cancellationToken = default);
    }
}