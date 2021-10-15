using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Models;

namespace Vpiska.Mongo
{
    internal sealed class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public MongoUserRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _collection = database.GetCollection<User>("users");
        }
        
        public async Task<Guid> Create(User user)
        {
            await _collection.InsertOneAsync(user);
            return user.Id;
        }
        
        public Task<User> GetById(Guid id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            return _collection.Find(filter).FirstOrDefaultAsync();
        }
        
        public Task<User> GetByPhone(string phone)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            return _collection.Find(filter).FirstOrDefaultAsync();
        }
        
        public Task<List<User>> GetByNameAndPhone(string name, string phone)
        {
            var nameFilter = Builders<User>.Filter.Eq(x => x.Name, name);
            var phoneFilter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var filter = Builders<User>.Filter.Or(nameFilter, phoneFilter);
            return _collection.Find(filter).ToListAsync();
        }
        
        public async Task<bool> SetCode(string phone, int code)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var update = Builders<User>.Update.Set(x => x.VerificationCode, code);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
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

        public async Task<bool> ChangePassword(Guid id, string password)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var update = Builders<User>.Update.Set(x => x.Password, password);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
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