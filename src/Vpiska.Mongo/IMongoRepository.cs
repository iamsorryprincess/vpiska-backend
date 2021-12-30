using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Mongo
{
    public interface IMongoRepository<TModel> where TModel : class, new()
    {
        Task InsertAsync(TModel model, CancellationToken cancellationToken = default);

        Task<TModel> GetAsync<TField>(Expression<Func<TModel, TField>> expression,
            TField value,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync<TSearchField, TUpdatedField>(Expression<Func<TModel, TSearchField>> searchExpression,
            TSearchField searchValue,
            Expression<Func<TModel, TUpdatedField>> updateExpression,
            TUpdatedField updateValue,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync<TField>(Expression<Func<TModel, TField>> expression,
            TField value,
            CancellationToken cancellationToken = default);

        Task<bool> CheckAsync<TField>(Expression<Func<TModel, TField>> expression,
            TField value,
            CancellationToken cancellationToken = default);

        Task<TModel> WhereSingleAsync(Expression<Func<TModel, bool>> expression,
            CancellationToken cancellationToken = default);

        Task<List<TProjection>> WhereProjectListAsync<TProjection>(Expression<Func<TModel, bool>> expression,
            Expression<Func<TModel, TProjection>> projectionExpression,
            CancellationToken cancellationToken = default);
    }
}