using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.UserAggregate;
using Vpiska.Domain.UserAggregate.Repository;

namespace Vpiska.Mongo.UserRepository
{
    internal sealed class UpdateUserRepository : IUpdateUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public UpdateUserRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _collection = database.GetCollection<User>("users");
        }
        
        public Task<bool> IsPhoneExist(string phone)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            return _collection.Find(filter).AnyAsync();
        }

        public Task<bool> IsNameExist(string name)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Name, name);
            return _collection.Find(filter).AnyAsync();
        }

        public async Task<bool> Update(Guid id, string name, string phone, string imageId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var updates = new List<UpdateDefinition<User>>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                updates.Add(Builders<User>.Update.Set(x => x.Name, name));
            }
            
            if (!string.IsNullOrWhiteSpace(phone))
            {
                updates.Add(Builders<User>.Update.Set(x => x.Phone, phone));
            }
            
            if (!string.IsNullOrWhiteSpace(imageId))
            {
                updates.Add(Builders<User>.Update.Set(x => x.ImageId, imageId));
            }

            if (!updates.Any())
            {
                return true;
            }

            var update = Builders<User>.Update.Combine(updates);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}