using System;

namespace Vpiska.Domain.EventAggregate.Events.Interfaces
{
    public interface ISubscriptionProvider<TEvent> where TEvent : IDomainEvent
    {
        void Subscribe(Guid eventId);

        bool TryUnsubscribe(Guid eventId);
    }
}