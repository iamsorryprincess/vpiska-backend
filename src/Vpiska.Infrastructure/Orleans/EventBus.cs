using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.Orleans
{
    internal sealed class EventBus : IEventBus
    {
        private readonly Subject<IDomainEvent> _subject = new();
        private readonly IClusterClient _clusterClient;

        public EventBus(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public IObservable<IDomainEvent> EventStream => _subject;

        public void Publish(IDomainEvent domainEvent) => _subject.OnNext(domainEvent);
        
        public Task SubscribeAsync(string eventId)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.SubscribeAsync(Constants.EventStreamNamespace);
        }

        public Task UnsubscribeAsync(string eventId)
        {
            var grain = _clusterClient.GetGrain<IEventGrain>(eventId);
            return grain.UnsubscribeAsync(Constants.EventStreamNamespace);
        }
    }
}