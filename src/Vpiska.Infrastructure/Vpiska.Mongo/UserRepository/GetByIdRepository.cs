using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.UserAggregate;
using Vpiska.Domain.UserAggregate.Repository;

namespace Vpiska.Mongo.UserRepository
{
    internal sealed class GetByIdRepository : IGetByIdRepository
    {
        private readonly IMongoCollection<User> _collection;

        public GetByIdRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _collection = database.GetCollection<User>("users");
        }
        
        public Task<User> GetById(Guid id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            return _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}