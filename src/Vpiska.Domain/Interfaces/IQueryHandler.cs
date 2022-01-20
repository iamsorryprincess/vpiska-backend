using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface IQueryHandler<in TQuery, TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}