using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Vpiska.IntegrationTests.Common
{
    public abstract class DatabaseTests
    {
        protected string DatabaseName { get; }
        
        protected IMongoClient MongoClient { get; }
        
        static DatabaseTests()
        {
            var conventionPack = new ConventionPack()
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("default", conventionPack, _ => true);
        }

        protected DatabaseTests()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("testsettings.json").Build();
            var mongoSection = configuration.GetSection("Mongo");
            DatabaseName = mongoSection["DbTestsName"];
            MongoClient = new MongoClient(mongoSection["ConnectionString"]);
        }
    }
}