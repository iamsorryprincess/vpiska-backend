using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Events.Interfaces
{
    public interface IEventConsumer<in TEvent> where TEvent : IDomainEvent
    {
        Task Consume(Guid eventId, TEvent @event);
    }
}