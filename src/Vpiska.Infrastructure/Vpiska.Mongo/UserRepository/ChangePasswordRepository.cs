using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.UserAggregate;
using Vpiska.Domain.UserAggregate.Repository;

namespace Vpiska.Mongo.UserRepository
{
    internal sealed class ChangePasswordRepository : IChangePasswordRepository
    {
        private readonly IMongoCollection<User> _collection;

        public ChangePasswordRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _collection = database.GetCollection<User>("users");
        }
        
        public async Task<bool> ChangePassword(Guid id, string password)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var update = Builders<User>.Update.Set(x => x.Password, password);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }
    }
}