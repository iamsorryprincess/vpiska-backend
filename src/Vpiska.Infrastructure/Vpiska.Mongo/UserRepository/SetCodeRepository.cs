using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.UserAggregate;
using Vpiska.Domain.UserAggregate.Repository;

namespace Vpiska.Mongo.UserRepository
{
    internal sealed class SetCodeRepository : ISetCodeRepository
    {
        private readonly IMongoCollection<User> _collection;

        public SetCodeRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _collection = database.GetCollection<User>("users");
        }
        
        public async Task<bool> SetCode(string phone, int code)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var update = Builders<User>.Update.Set(x => x.VerificationCode, code);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }
    }
}