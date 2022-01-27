using System;

namespace Vpiska.Domain.Event.Interfaces
{
    public interface IEventBus
    {
        IObservable<IDomainEvent> EventStream { get; }

        void Publish(IDomainEvent domainEvent);
    }
}