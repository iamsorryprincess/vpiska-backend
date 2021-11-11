using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.EventAggregate.Events.Interfaces
{
    public interface IEventProducer<in TEvent> where TEvent : IDomainEvent
    {
        Task Produce(Guid eventId, TEvent @event);
    }
}