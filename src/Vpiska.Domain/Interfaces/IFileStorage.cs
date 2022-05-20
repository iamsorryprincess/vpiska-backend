using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vpiska.Domain.Common;

namespace Vpiska.Domain.Interfaces
{
    public interface IFileStorage
    {
        Task<MediaResult> SaveFileAsync(string filename, string contentType, Stream fileStream,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(string filename, CancellationToken cancellationToken = default);
    }
}