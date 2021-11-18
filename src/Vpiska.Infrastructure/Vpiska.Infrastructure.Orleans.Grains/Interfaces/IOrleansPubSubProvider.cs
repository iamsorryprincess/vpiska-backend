using System.Threading.Tasks;

namespace Vpiska.Infrastructure.Orleans.Grains.Interfaces
{
    public interface IOrleansPubSubProvider<T>
    {
        Task<bool> TrySubscribe(string eventId);

        Task<bool> TryUnsubscribe(string eventId);
    }
}