using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Infrastructure.Database
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
    {
        protected readonly IMongoCollection<TEntity> Collection;

        protected RepositoryBase(IMongoClient mongoClient, string databaseName, string collectionName)
        {
            var db = mongoClient.GetDatabase(databaseName);
            Collection = db.GetCollection<TEntity>(collectionName);
        }
        
        public Task<TEntity> GetByFieldAsync<TValue>(string fieldName, TValue fieldValue, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TEntity>.Filter.Eq(fieldName, fieldValue);
            return Collection.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

        public async Task<bool> UpdateByFieldAsync<TSearchValue, TUpdateValue>(string fieldName, TSearchValue searchValue, string updateField,
            TUpdateValue updateValue, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TEntity>.Filter.Eq(fieldName, searchValue);
            var update = Builders<TEntity>.Update.Set(updateField, updateValue);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        public async Task<bool> RemoveByFieldAsync<TSearchValue>(string fieldName, TSearchValue searchValue,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TEntity>.Filter.Eq(fieldName, searchValue);
            var result = await Collection.DeleteOneAsync(filter, cancellationToken);
            return result.DeletedCount > 0;
        }

        public Task<bool> CheckByFieldAsync<TValue>(string fieldName, TValue fieldValue, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TEntity>.Filter.Eq(fieldName, fieldValue);
            return Collection.Find(filter).AnyAsync(cancellationToken: cancellationToken);
        }
    }
}