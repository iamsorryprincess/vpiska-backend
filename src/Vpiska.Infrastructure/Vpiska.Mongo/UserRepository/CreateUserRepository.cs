using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.UserAggregate;
using Vpiska.Domain.UserAggregate.Repository;

namespace Vpiska.Mongo.UserRepository
{
    internal sealed class CreateUserRepository : ICreateUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public CreateUserRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _collection = database.GetCollection<User>("users");
        }
        
        public Task<List<User>> GetByNameAndPhone(string name, string phone)
        {
            var nameFilter = Builders<User>.Filter.Eq(x => x.Name, name);
            var phoneFilter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var filter = Builders<User>.Filter.Or(nameFilter, phoneFilter);
            return _collection.Find(filter).ToListAsync();
        }

        public async Task<Guid> Create(User user)
        {
            await _collection.InsertOneAsync(user);
            return user.Id;
        }
    }
}