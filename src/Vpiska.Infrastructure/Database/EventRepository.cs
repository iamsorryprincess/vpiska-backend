using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.Database
{
    internal sealed class EventRepository : RepositoryBase<Event>, IEventRepository
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

        public async Task<bool> AddMediaLink(string eventId, string mediaLink, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.Eq(x => x.Id, eventId);
            var update = Builders<Event>.Update.AddToSet(x => x.MediaLinks, mediaLink);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        public async Task<bool> RemoveMediaLink(string eventId, string mediaLink, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Event>.Filter.Eq(x => x.Id, eventId);
            var update = Builders<Event>.Update.Pull(x => x.MediaLinks, mediaLink);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }
    }
}