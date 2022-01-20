using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Domain.Interfaces
{
    public interface ICommandHandler<in TCommand, TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }

    public interface ICommandHandler<in TCommand>
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}