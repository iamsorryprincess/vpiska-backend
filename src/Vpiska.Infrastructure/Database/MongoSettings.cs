namespace Vpiska.Infrastructure.Database
{
    internal sealed class MongoSettings
    {
        public string DatabaseName { get; }

        public MongoSettings(string databaseName)
        {
            DatabaseName = databaseName;
        }
    }
}