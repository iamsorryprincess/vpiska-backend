using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using MongoDB.Driver;
using Vpiska.Domain;
using Vpiska.Domain.Models;

namespace Vpiska.Mongo
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        
        public UserRepository(MongoClient client)
        {
            var database = client.GetDatabase("vpiska");
            _users = database.GetCollection<User>("users");
        }
        
        public async Task<FSharpValueOption<User>> GetById(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            return user == null ? FSharpValueOption<User>.None : FSharpValueOption<User>.Some(user);
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

        public async Task<CheckPhoneNameResult> CheckInfo(string phone, string name)
        {
            var nameFilter = Builders<User>.Filter.Eq(x => x.Name, name);
            var phoneFilter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var filter = Builders<User>.Filter.Or(nameFilter, phoneFilter);

            var result = await _users.Find(filter)
                .Project(user => new CheckPhoneNameResult(user.Phone == phone, user.Name == name))
                .ToListAsync();

            if (result.Count == 0)
            {
                return new CheckPhoneNameResult(false, false);
            }

            return result.Aggregate(new CheckPhoneNameResult(false, false), (acc, item) =>
            {
                if (!acc.IsNameExist)
                {
                    acc = new CheckPhoneNameResult(acc.IsPhoneExist, item.IsNameExist);
                }

                if (!acc.IsPhoneExist)
                {
                    acc = new CheckPhoneNameResult(item.IsPhoneExist, acc.IsNameExist);
                }

                return acc;
            });
        }

        public async Task<FSharpValueOption<User>> GetUserByPhone(string phone)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            return user == null ? FSharpValueOption<User>.None : FSharpValueOption<User>.Some(user);
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