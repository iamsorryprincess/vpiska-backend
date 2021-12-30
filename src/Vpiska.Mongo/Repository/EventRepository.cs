using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Models;

namespace Vpiska.Mongo.Repository
{
    internal sealed class EventRepository : MongoRepository<Event>, IEventRepository
    {
        public EventRepository(IMongoClient client, MongoSettings settings) : base(client, settings)
        {
        }

        public Task<bool> CheckForOwnershipAsync(string eventId, string ownerId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.And(Builders<Event>.Filter.Eq(x => x.Id, eventId),
                Builders<Event>.Filter.Eq(x => x.OwnerId, ownerId));
            return Collection.Find(filter).AnyAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> AddMediaAsync(string eventId, string mediaId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.Eq(x => x.Id, eventId);
            var update = Builders<Event>.Update.AddToSet(x => x.MediaLinks, mediaId);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        public Task<bool> CheckMediaAsync(string eventId, string mediaId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.And(Builders<Event>.Filter.Eq(x => x.Id, eventId),
                Builders<Event>.Filter.AnyEq(x => x.MediaLinks, mediaId));
            return Collection.Find(filter).AnyAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteMediaAsync(string eventId, string mediaId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.Eq(x => x.Id, eventId);
            var update = Builders<Event>.Update.Pull(x => x.MediaLinks, mediaId);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }
    }
}