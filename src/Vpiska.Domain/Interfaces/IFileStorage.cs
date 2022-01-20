using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveFileAsync(string filename, string contentType, Stream fileStream,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(string filename, CancellationToken cancellationToken = default);
    }
}