using System.Threading.Tasks;
using Vpiska.Domain.Event;

namespace Vpiska.Infrastructure.Orleans.Interfaces
{
    public interface IStreamConsumer
    {
        Task Consume(string eventId, DomainEvent domainEvent);
    }
}