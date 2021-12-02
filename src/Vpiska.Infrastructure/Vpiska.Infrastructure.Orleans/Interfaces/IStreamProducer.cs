using System.Threading.Tasks;
using Vpiska.Domain.Event;

namespace Vpiska.Infrastructure.Orleans.Interfaces
{
    public interface IStreamProducer
    {
        Task<bool> TrySubscribe(string eventId);

        Task<bool> TryUnsubscribe(string eventId);

        Task Produce(string eventId, DomainEvent domainEvent);
    }
}