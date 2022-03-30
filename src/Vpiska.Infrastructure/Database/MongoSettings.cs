namespace Vpiska.Infrastructure.Database
{
    public sealed class MongoSettings
    {
        public string DatabaseName { get; }

        public MongoSettings(string databaseName)
        {
            DatabaseName = databaseName;
        }
    }
}