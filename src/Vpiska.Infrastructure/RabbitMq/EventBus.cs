using System;
using System.Reactive.Subjects;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.RabbitMq
{
    internal sealed class EventBus : IEventBus
    {
        private readonly Subject<IDomainEvent> _subject;

        public IObservable<IDomainEvent> EventStream => _subject;

        public EventBus()
        {
            _subject = new Subject<IDomainEvent>();
        }

        public void Publish(IDomainEvent domainEvent) => _subject.OnNext(domainEvent);
    }
}