using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Firebase
{
    internal sealed class FirebaseStorage : IFirebaseStorage
    {
        private const string BucketName = "vpiska-a6e65.appspot.com";
        
        private readonly StorageClient _client;

        public FirebaseStorage(StorageClient client)
        {
            _client = client;
        }
        
        public async Task<string> UploadFile(string fileName, string contentType, Stream stream)
        {
            var image = await _client.UploadObjectAsync(BucketName, fileName,
                contentType, stream);
            return image.Name;
        }
    }
}