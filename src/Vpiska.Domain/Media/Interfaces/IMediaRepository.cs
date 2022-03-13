using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media.Models;

namespace Vpiska.Domain.Media.Interfaces
{
    public interface IMediaRepository : IRepository<Media>
    {
        Task<bool> UpdateAsync(Media media, CancellationToken cancellationToken = default);

        Task<PagedResponse> GetPagedFilesMetadata(int page, int size,
            CancellationToken cancellationToken = default);
    }
}