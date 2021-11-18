using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Streams;
using Vpiska.Infrastructure.Orleans.Grains.Interfaces;

namespace Vpiska.Infrastructure.Orleans.Grains
{
    internal sealed class OrleansPubSubProvider<T> : IOrleansPubSubProvider<T>
    {
        private readonly IClusterClient _clusterClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, StreamSubscriptionHandle<T>> _subscriptions;

        public OrleansPubSubProvider(IClusterClient clusterClient, IServiceScopeFactory scopeFactory)
        {
            _clusterClient = clusterClient;
            _scopeFactory = scopeFactory;
            _subscriptions = new ConcurrentDictionary<string, StreamSubscriptionHandle<T>>();
        }

        public async Task<bool> TrySubscribe(string eventId)
        {
            if (_subscriptions.ContainsKey(eventId))
            {
                return false;
            }

            var stream = GetStream(eventId);
            
            var subscription = await stream.SubscribeAsync<T>(async (data, token) =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer<T>>();
                await consumer.Consume(eventId, data);
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

        public Task Publish(string eventId, T data)
        {
            var stream = GetStream(eventId);
            return stream.OnNextAsync(data);
        }

        private IAsyncStream<T> GetStream(string eventId)
        {
            var streamProvider = _clusterClient.GetStreamProvider("chatProvider");
            return streamProvider.GetStream<T>(Guid.Parse(eventId), "chat");
        }
    }
}