﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Models;

namespace Vpiska.Mongo
{
    public static class Entry
    {
        public static void AddMongo(this IServiceCollection services, IConfigurationSection mongoSection)
        {
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            
            ConventionRegistry.Register("default", conventionPack, t => true);
            
            BsonClassMap.RegisterClassMap<User>(cm => 
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(GuidGenerator.Instance);
            });

            var client = new MongoClient(mongoSection["ConnectionString"]);
            services.AddSingleton(client);
            services.AddTransient<IUserRepository, MongoUserRepository>();
        }
    }
}