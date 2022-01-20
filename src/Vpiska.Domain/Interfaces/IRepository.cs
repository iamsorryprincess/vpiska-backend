using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface IRepository<TEntity>
    {
        Task<TEntity> GetByFieldAsync<TValue>(string fieldName, TValue fieldValue,
            CancellationToken cancellationToken = default);

        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<bool> UpdateByFieldAsync<TSearchValue, TUpdateValue>(string fieldName, TSearchValue searchValue,
            string updateField, TUpdateValue updateValue, CancellationToken cancellationToken = default);

        Task<bool> RemoveByFieldAsync<TSearchValue>(string fieldName, TSearchValue searchValue,
            CancellationToken cancellationToken = default);

        Task<bool> CheckByFieldAsync<TValue>(string fieldName, TValue fieldValue,
            CancellationToken cancellationToken = default);
    }
}