using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.UserAggregate;
using Vpiska.Domain.UserAggregate.Repository;

namespace Vpiska.Mongo.UserRepository
{
    internal sealed class GetByPhoneRepository : IGetByPhoneRepository
    {
        private readonly IMongoCollection<User> _collection;

        public GetByPhoneRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _collection = database.GetCollection<User>("users");
        }
        
        public Task<User> GetByPhone(string phone)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            return _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}