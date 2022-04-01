using System.Threading.Tasks;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventBus
    {
        ValueTask PublishAsync(IDomainEvent domainEvent);
    }
}