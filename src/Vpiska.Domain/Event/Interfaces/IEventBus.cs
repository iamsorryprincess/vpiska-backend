using System;
using System.Threading.Tasks;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventBus
    {
        IObservable<IDomainEvent> EventStream { get; }

        void Publish(IDomainEvent domainEvent);

        Task SubscribeAsync(string eventId);

        Task UnsubscribeAsync(string eventId);
    }
}