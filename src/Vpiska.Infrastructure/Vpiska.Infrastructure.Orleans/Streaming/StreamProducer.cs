using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Streams;
using Serilog;
using Vpiska.Domain.Event;
using Vpiska.Infrastructure.Orleans.Interfaces;

namespace Vpiska.Infrastructure.Orleans.Streaming
{
    internal sealed class StreamProducer : IStreamProducer
    {
        private const string StreamProviderName = "chatProvider";
        private const string StreamNamespace = "chat";
        
        private readonly ILogger _logger;
        private readonly IClusterClient _clusterClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, StreamSubscriptionHandle<DomainEvent>> _subscriptions;

        public StreamProducer(ILogger logger,
            IClusterClient clusterClient,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _clusterClient = clusterClient;
            _scopeFactory = scopeFactory;
            _subscriptions = new ConcurrentDictionary<string, StreamSubscriptionHandle<DomainEvent>>();
        }

        public async Task<bool> TrySubscribe(string eventId)
        {
            if (_subscriptions.ContainsKey(eventId))
            {
                return false;
            }

            var stream = GetStream(eventId);

            var subscription = await stream.SubscribeAsync(async (domainEvent, token) =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var consumer = scope.ServiceProvider.GetRequiredService<IStreamConsumer>();
                await consumer.Consume(eventId, domainEvent);
            }, (ex) =>
            {
                _logger.Error(ex, "error while consuming event");
                return Task.CompletedTask;
            });

            return _subscriptions.TryAdd(eventId, subscription);
        }

        public async Task<bool> TryUnsubscribe(string eventId)
        {
            if (!_subscriptions.TryRemove(eventId, out var subscription))
            {
                return false;
            }

            await subscription.UnsubscribeAsync();
            return true;
        }

        public Task Produce(string eventId, DomainEvent domainEvent)
        {
            var stream = GetStream(eventId);
            return stream.OnNextAsync(domainEvent);
        }

        private IAsyncStream<DomainEvent> GetStream(string eventId)
        {
            var streamProvider = _clusterClient.GetStreamProvider(StreamProviderName);
            return streamProvider.GetStream<DomainEvent>(Guid.ParseExact(eventId, "N"), StreamNamespace);
        }
    }
}