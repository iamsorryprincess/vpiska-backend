using System.Threading.Channels;
using System.Threading.Tasks;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.RabbitMq
{
    internal sealed class EventBus : IEventBus
    {
        private readonly Channel<IDomainEvent> _channel;

        public EventBus(Channel<IDomainEvent> channel)
        {
            _channel = channel;
        }

        public ValueTask PublishAsync(IDomainEvent domainEvent) => _channel.Writer.WriteAsync(domainEvent);
    }
}