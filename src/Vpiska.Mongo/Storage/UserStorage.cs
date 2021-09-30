using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Mongo.Interfaces;
using Vpiska.Mongo.Models;

namespace Vpiska.Mongo.Storage
{
    public sealed class UserStorage : IUserStorage
    {
        private readonly IMongoCollection<User> _users;

        public UserStorage(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _users = database.GetCollection<User>("users");
        }

        public Task<User> GetById(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            return _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<string> Create(User user)
        {
            await _users.InsertOneAsync(user);
            return user.Id;
        }

        public async Task<bool> Update(string id, string name, string phone, string imageUrl)
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
            
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                updates.Add(Builders<User>.Update.Set(x => x.ImageUrl, imageUrl));
            }

            if (!updates.Any())
            {
                return true;
            }

            var update = Builders<User>.Update.Combine(updates);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }

        public async Task<NamePhoneCheckResult> CheckInfo(string name, string phone)
        {
            var nameFilter = Builders<User>.Filter.Eq(x => x.Name, name);
            var phoneFilter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var filter = Builders<User>.Filter.Or(nameFilter, phoneFilter);
            
            var result = await _users.Find(filter).Project(user => new NamePhoneCheckResult()
            {
                IsNameExist = user.Name == name,
                IsPhoneExist = user.Phone == phone
            }).ToListAsync();

            if (result.Count == 0)
            {
                return null;
            }

            return result.Aggregate(new NamePhoneCheckResult(), (acc, item) =>
            {
                if (!acc.IsNameExist)
                {
                    acc.IsNameExist = item.IsNameExist;
                }

                if (!acc.IsPhoneExist)
                {
                    acc.IsPhoneExist = item.IsPhoneExist;
                }

                return acc;
            });
        }

        public Task<User> GetUserByPhone(string phone)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            return _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> SetVerificationCode(string phone, int code)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var update = Builders<User>.Update.Set(x => x.VerificationCode, code);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }

        public async Task<bool> ChangePassword(string id, string password)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var update = Builders<User>.Update.Set(x => x.Password, password);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }
    }
}