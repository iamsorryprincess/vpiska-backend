using System.IO;
using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface IFileStorage
    {
        Task<string> UploadFile(string fileName, string contentType, Stream stream);

        Task<bool> DeleteFile(string url);
    }
}