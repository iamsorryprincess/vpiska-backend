using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Vpiska.Domain.UserAggregate;
using Vpiska.Domain.UserAggregate.Repository;
using Vpiska.Mongo.UserRepository;

namespace Vpiska.Mongo
{
    public static class Entry
    {
        public static void AddMongo(this IServiceCollection services, IConfigurationSection mongoSection)
        {
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new ImmutableTypeClassMapConvention()
            };
            
            ConventionRegistry.Register("default", conventionPack, t => true);
            
            BsonClassMap.RegisterClassMap<User>(cm => 
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });

            services.AddSingleton(new MongoClient(mongoSection["ConnectionString"]));
            services.AddTransient<IChangePasswordRepository, ChangePasswordRepository>();
            services.AddTransient<ICreateUserRepository, CreateUserRepository>();
            services.AddTransient<IGetByIdRepository, GetByIdRepository>();
            services.AddTransient<IGetByPhoneRepository, GetByPhoneRepository>();
            services.AddTransient<ISetCodeRepository, SetCodeRepository>();
            services.AddTransient<IUpdateUserRepository, UpdateUserRepository>();
        }
    }
}