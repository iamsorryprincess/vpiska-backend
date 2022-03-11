using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Media;
using Vpiska.Domain.Media.Interfaces;

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
    }
}