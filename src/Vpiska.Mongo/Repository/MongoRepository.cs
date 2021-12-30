using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Vpiska.Mongo.Repository
{
    internal class MongoRepository<TModel> : IMongoRepository<TModel> where TModel : class, new()
    {
        protected readonly IMongoCollection<TModel> Collection;

        public MongoRepository(IMongoClient client, MongoSettings settings)
        {
            var database = client.GetDatabase(settings.DatabaseName);
            Collection = database.GetCollection<TModel>($"{typeof(TModel).Name.ToLower()}s");
        }

        public Task InsertAsync(TModel model, CancellationToken cancellationToken = default) =>
            Collection.InsertOneAsync(model, cancellationToken: cancellationToken);

        public Task<TModel> GetAsync<TField>(Expression<Func<TModel, TField>> expression,
            TField value,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TModel>.Filter.Eq(expression, value);
            return Collection.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> UpdateAsync<TSearchField, TUpdatedField>(Expression<Func<TModel, TSearchField>> searchExpression,
            TSearchField searchValue,
            Expression<Func<TModel, TUpdatedField>> updateExpression,
            TUpdatedField updateValue,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TModel>.Filter.Eq(searchExpression, searchValue);
            var update = Builders<TModel>.Update.Set(updateExpression, updateValue);
            var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync<TField>(Expression<Func<TModel, TField>> expression,
            TField value,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TModel>.Filter.Eq(expression, value);
            var result = await Collection.DeleteOneAsync(filter, cancellationToken);
            return result.DeletedCount > 0;
        }

        public Task<bool> CheckAsync<TField>(Expression<Func<TModel, TField>> expression,
            TField value,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TModel>.Filter.Eq(expression, value);
            return Collection.Find(filter).AnyAsync(cancellationToken: cancellationToken);
        }

        public Task<TModel> WhereSingleAsync(Expression<Func<TModel, bool>> expression,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TModel>.Filter.Where(expression);
            return Collection.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
        
        public Task<List<TProjection>> WhereProjectListAsync<TProjection>(Expression<Func<TModel, bool>> expression,
            Expression<Func<TModel, TProjection>> projectionExpression,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TModel>.Filter.Where(expression);
            return Collection.Find(filter)
                .Project(projectionExpression)
                .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}