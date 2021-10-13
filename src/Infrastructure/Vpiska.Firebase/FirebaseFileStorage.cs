using System.IO;
using System.Net;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;

namespace Vpiska.Firebase
{
    public sealed class FirebaseFileStorage
    {
        private const string BucketName = "vpiska-a6e65.appspot.com";
        
        private readonly StorageClient _client;

        public FirebaseFileStorage(StorageClient client)
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