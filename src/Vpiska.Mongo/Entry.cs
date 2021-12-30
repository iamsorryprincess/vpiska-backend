using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Vpiska.Domain.Models;
using Vpiska.Mongo.Repository;

namespace Vpiska.Mongo
{
    public static class Entry
    {
        public static void AddMongo(this IServiceCollection services, IConfigurationSection mongoSection)
        {
            var conventionPack = new ConventionPack()
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("default", conventionPack, _ => true);
            
            BsonClassMap.RegisterClassMap<User>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });
            
            BsonClassMap.RegisterClassMap<Event>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });

            var mongoClient = new MongoClient(mongoSection["ConnectionString"]);
            services.AddSingleton<IMongoClient>(mongoClient);
            services.AddSingleton(new MongoSettings()
            {
                DatabaseName = mongoSection["DatabaseName"]
            });
            services.AddTransient(typeof(IMongoRepository<>), typeof(MongoRepository<>));
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IEventRepository, EventRepository>();
        }
    }
}