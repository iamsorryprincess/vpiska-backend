using System.Threading.Channels;
using System.Threading.Tasks;

namespace Vpiska.Kafka
{
    internal sealed class KafkaProducer<TEvent> : IProducer<TEvent>
    {
        private readonly Channel<TEvent> _channel;

        public KafkaProducer(Channel<TEvent> channel)
        {
            _channel = channel;
        }

        public ValueTask ProduceAsync(TEvent queueEvent) => _channel.Writer.WriteAsync(queueEvent);
    }
}