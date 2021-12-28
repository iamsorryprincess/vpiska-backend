using System.Threading;
using System.Threading.Tasks;

namespace Vpiska.Kafka
{
    public interface IConsumer<in TEvent>
    {
        Task ConsumeAsync(TEvent queueEvent, CancellationToken cancellationToken = default);
    }
}