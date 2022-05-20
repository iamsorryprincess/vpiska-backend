using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;

namespace Vpiska.Infrastructure.Database
{
    public sealed class EventRepository : RepositoryBase<Event>, IEventRepository
    {
        public EventRepository(IMongoClient mongoClient, MongoSettings settings) : base(mongoClient,
            settings.DatabaseName, "events")
        {
        }

        public Task<List<Event>> GetAll(CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.Empty;
            return Collection.Find(filter).ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> AddMediaLink(string eventId, MediaInfo mediaInfo, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.Eq(x => x.Id, eventId);
            var update = Builders<Event>.Update.AddToSet(x => x.Media, mediaInfo);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        public async Task<bool> RemoveMediaLink(string eventId, string mediaId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.Eq(x => x.Id, eventId);
            var update =
                Builders<Event>.Update.PullFilter(x => x.Media, Builders<MediaInfo>.Filter.Eq(x => x.Id, mediaId));
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }
    }
}