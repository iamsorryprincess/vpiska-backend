using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Infrastructure.Firebase
{
    internal sealed class FileStorage : IFileStorage
    {
        private readonly FirebaseSettings _settings;
        private readonly StorageClient _storageClient;

        public FileStorage(FirebaseSettings settings, StorageClient storageClient)
        {
            _settings = settings;
            _storageClient = storageClient;
        }
        
        public async Task<MediaResult> SaveFileAsync(string filename, string contentType, Stream fileStream,
            CancellationToken cancellationToken = default)
        {
            var result = await _storageClient.UploadObjectAsync(_settings.BucketName, filename, contentType, fileStream,
                cancellationToken: cancellationToken);
            if (fileStream != null)
            {
                await fileStream.DisposeAsync();
            }
            return new MediaResult()
            {
                Id = result.Name,
                ContentType = contentType
            };
        }

        public async Task<bool> DeleteFileAsync(string filename, CancellationToken cancellationToken = default)
        {
            try
            {
                await _storageClient.DeleteObjectAsync(_settings.BucketName, filename,
                    cancellationToken: cancellationToken);
                return true;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }
        }
    }
}