using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Orleans.Grains.Interfaces
{
    public interface IEventConsumer<in TData>
    {
        Task Consume(string eventId, TData data);
    }
}