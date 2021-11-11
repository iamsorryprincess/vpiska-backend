using System.Threading.Tasks;
using Orleans;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Orleans.EventBus.Constants;

namespace Vpiska.Orleans.EventBus
{
    internal sealed class ChatBus
    {
        private readonly IClusterClient _clusterClient;

        public ChatBus(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        
        public Task Publish(ChatMessageRequest domainEvent)
        {
            var streamProvider = _clusterClient.GetStreamProvider(StreamsConstants.ChatProvider);
            var stream = streamProvider.GetStream<ChatMessageRequest>(domainEvent.EventId, StreamsConstants.ChatProvider);
            return stream.OnNextAsync(domainEvent);
        }
    }
}