using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Models;

namespace Vpiska.Mongo.Repository
{
    internal sealed class UserRepository : MongoRepository<User>, IUserRepository
    {
        public UserRepository(IMongoClient client, MongoSettings settings) : base(client, settings)
        {
        }

        public async Task<(bool isPhoneExist, bool isNameExist)> CheckPhoneAndName(string phone, string name,
            CancellationToken cancellationToken = default)
        {
            var isNameEmpty = string.IsNullOrWhiteSpace(name);
            var isPhoneEmpty = string.IsNullOrWhiteSpace(phone);
            var filter = isNameEmpty switch
            {
                true when isPhoneEmpty => null,
                false when isPhoneEmpty => Builders<User>.Filter.Eq(x => x.Name, name),
                true => Builders<User>.Filter.Eq(x => x.Phone, phone),
                _ => Builders<User>.Filter.Or(Builders<User>.Filter.Eq(x => x.Name, name),
                    Builders<User>.Filter.Eq(x => x.Phone, phone))
            };

            var (isPhoneExist, isNameExist) = filter == null
                ? (false, false)
                : (await Collection
                    .Find(filter)
                    .Project(x => new
                    {
                        IsPhoneExist = x.Phone == phone,
                        IsNameExist = x.Name == name
                    })
                    .ToListAsync(cancellationToken))
                .Aggregate((false, false), (acc, item) =>
                {
                    if (item.IsPhoneExist)
                        acc.Item1 = true;
                    if (item.IsNameExist)
                        acc.Item2 = true;
                    return acc;
                });

            return (isPhoneExist, isNameExist);
        }

        public async Task<bool> UpdateUser(string id, string name, string phone, string imageId,
            CancellationToken cancellationToken = default)
        {
            var updates = new List<UpdateDefinition<User>>();

            if (!string.IsNullOrWhiteSpace(name))
                updates.Add(Builders<User>.Update.Set(x => x.Name, name));
            if (!string.IsNullOrWhiteSpace(phone))
                updates.Add(Builders<User>.Update.Set(x => x.Phone, phone));
            if (!string.IsNullOrWhiteSpace(imageId))
                updates.Add(Builders<User>.Update.Set(x => x.ImageId, imageId));

            if (updates.Count == 0)
            {
                return true;
            }

            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var update = Builders<User>.Update.Combine(updates);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }
    }
}