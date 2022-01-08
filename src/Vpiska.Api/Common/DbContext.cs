using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Vpiska.Api.Models.User;
using Vpiska.Api.Settings;

namespace Vpiska.Api.Common
{
    public sealed class DbContext
    {
        public IMongoCollection<User> Users { get; }

        public DbContext(IMongoClient mongoClient, IOptions<MongoSettings> mongoSettings)
        {
            var db = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            Users = db.GetCollection<User>("users");
        }
    }
}