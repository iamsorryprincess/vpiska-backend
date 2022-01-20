using System.Threading.Tasks;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task Handle(TEvent domainEvent);
    }
}