using System.IO;
using System.Net;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;
using Vpiska.Domain;

namespace Vpiska.Firebase
{
    internal sealed class FileStorage : IFileStorage
    {
        private const string BucketName = "vpiska-a6e65.appspot.com";
        
        private readonly StorageClient _client;

        public FileStorage(StorageClient client)
        {
            _client = client;
        }
        
        public async Task<string> UploadFile(string fileName, string contentType, Stream stream)
        {
            var image = await _client.UploadObjectAsync(BucketName, fileName,
                contentType, stream);
            return image.Name;
        }

        public async Task<bool> DeleteFile(string url)
        {
            try
            {
                await _client.DeleteObjectAsync(BucketName, url);
                return true;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                return false;
            }
        }
    }
}