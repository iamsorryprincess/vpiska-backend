using System;
using System.Reactive.Subjects;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.Orleans
{
    internal sealed class EventBus : IEventBus
    {
        private readonly Subject<IDomainEvent> _subject = new();

        public IObservable<IDomainEvent> EventStream => _subject;

        public void Publish(IDomainEvent domainEvent) => _subject.OnNext(domainEvent);
    }
}