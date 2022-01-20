using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Responses;

namespace Vpiska.Infrastructure.Database
{
    internal sealed class EventRepository : RepositoryBase<Event>, IEventRepository
    {
        public EventRepository(IMongoClient mongoClient, MongoSettings settings) : base(mongoClient,
            settings.DatabaseName, "events")
        {
        }

        public Task<List<EventShortResponse>> GetByRange(
            double xLeft,
            double xRight,
            double yLeft,
            double yRight,
            CancellationToken cancellationToken = default)
        {
            var leftHorizontalFilter = Builders<Event>.Filter.Gte(x => x.Coordinates.X, xLeft);
            var rightHorizontalFilter = Builders<Event>.Filter.Lte(x => x.Coordinates.X, xRight);
            var leftVerticalFilter = Builders<Event>.Filter.Gte(x => x.Coordinates.Y, yLeft);
            var rightVerticalFilter = Builders<Event>.Filter.Lte(x => x.Coordinates.Y, yRight);
            var filter = Builders<Event>.Filter.And(leftHorizontalFilter, rightHorizontalFilter, leftVerticalFilter,
                rightVerticalFilter);
            return Collection.Find(filter)
                .Project(model => EventShortResponse.FromModel(model))
                .ToListAsync(cancellationToken: cancellationToken);
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