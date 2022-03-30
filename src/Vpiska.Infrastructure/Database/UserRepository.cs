using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Domain.User.Models;

namespace Vpiska.Infrastructure.Database
{
    public sealed class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(IMongoClient mongoClient, MongoSettings settings) : base(mongoClient,
            settings.DatabaseName, "users")
        {
        }

        public Task<CheckResult> CheckPhoneAndName(string phone, string name, CancellationToken cancellationToken = default)
        {
            var phoneFilter = Builders<User>.Filter.Eq(x => x.Phone, phone);
            var nameFilter = Builders<User>.Filter.Eq(x => x.Name, name);
            var checkFilter = Builders<User>.Filter.Or(phoneFilter, nameFilter);
            return Check(phone, name, checkFilter, cancellationToken);
        }

        public async Task<CheckResult> CheckPhoneAndNameWithEmptyParams(string phone, string name,
            CancellationToken cancellationToken = default)
        {
            var isNameEmpty = string.IsNullOrWhiteSpace(name);
            var isPhoneEmpty = string.IsNullOrWhiteSpace(phone);
            
            var checkFilter = isNameEmpty switch
            {
                true when isPhoneEmpty => null,
                false when isPhoneEmpty => Builders<User>.Filter.Eq(x => x.Name, name),
                true => Builders<User>.Filter.Eq(x => x.Phone, phone),
                _ => Builders<User>.Filter.Or(Builders<User>.Filter.Eq(x => x.Name, name),
                    Builders<User>.Filter.Eq(x => x.Phone, phone))
            };

            var checkResult = checkFilter == null
                ? new CheckResult()
                : await Check(phone, name, checkFilter, cancellationToken);

            return checkResult;
        }

        public async Task<bool> UpdateUser(string id, string name, string phone, string imageId,
            CancellationToken cancellationToken)
        {
            var isNameEmpty = string.IsNullOrWhiteSpace(name);
            var isPhoneEmpty = string.IsNullOrWhiteSpace(phone);
            
            var update = isNameEmpty switch
            {
                true when isPhoneEmpty => Builders<User>.Update.Set(x => x.ImageId, imageId),
                false when isPhoneEmpty => Builders<User>.Update.Combine(
                    Builders<User>.Update.Set(x => x.Name, name),
                    Builders<User>.Update.Set(x => x.ImageId, imageId)),
                true => Builders<User>.Update.Combine(Builders<User>.Update.Set(x => x.Phone, phone),
                    Builders<User>.Update.Set(x => x.ImageId, imageId)),
                _ => Builders<User>.Update.Combine(Builders<User>.Update.Set(x => x.Name, name),
                    Builders<User>.Update.Set(x => x.Phone, phone),
                    Builders<User>.Update.Set(x => x.ImageId, imageId))
            };

            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        private async Task<CheckResult> Check(string phone, string name, FilterDefinition<User> checkFilter, CancellationToken cancellationToken)
        {
            var checkResult = await Collection
                .Find(checkFilter)
                .Project(x => new CheckResult() { IsPhoneExist = x.Phone == phone, IsNameExist = x.Name == name })
                .ToListAsync(cancellationToken: cancellationToken);

            return checkResult.Aggregate(new CheckResult(), (acc, item) =>
            {
                if (item.IsPhoneExist)
                    acc.IsPhoneExist = true;
                if (item.IsNameExist)
                    acc.IsNameExist = true;
                return acc;
            });
        }
    }
}