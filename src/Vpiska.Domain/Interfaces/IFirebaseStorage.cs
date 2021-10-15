using System.IO;
using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface IFirebaseStorage
    {
        Task<string> UploadFile(string fileName, string contentType, Stream stream);
    }
}