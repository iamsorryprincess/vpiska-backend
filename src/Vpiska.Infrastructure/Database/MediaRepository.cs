using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Media;
using Vpiska.Domain.Media.Interfaces;
using Vpiska.Domain.Media.Models;

namespace Vpiska.Infrastructure.Database
{
    internal sealed class MediaRepository : RepositoryBase<Media>, IMediaRepository
    {
        public MediaRepository(IMongoClient mongoClient, MongoSettings settings) : base(mongoClient,
            settings.DatabaseName, "media")
        {
        }

        public async Task<bool> UpdateAsync(Media media, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Media>.Filter.Eq(x => x.Id, media.Id);
            var result = await Collection.ReplaceOneAsync(filter, media, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        public async Task<PagedResponse> GetPagedFilesMetadata(int page, int size,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<Media>.Filter.Empty;
            var count = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            var rem = count % size;

            var totalPages = rem == 0
                ? count / size
                : count / size + 1;
            
            var data = await Collection.Find(filter)
                .Skip((page - 1) * size)
                .Limit(size)
                .Project(media => new MetadataViewModel()
                {
                    Name = media.Name,
                    Size = media.Size.MapToSize(),
                    Type = media.ContentType,
                    LastModified = media.LastModifiedDate.ToString("dd.MM.yyyy")
                })
                .ToListAsync(cancellationToken: cancellationToken);
            
            return new PagedResponse()
            {
                Page = page,
                ItemsPerPage = size,
                TotalPages = (int)totalPages,
                TotalItems = (int)count,
                FilesMetadata = data
            };
        }
    }
}