using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vpiska.Kafka.Settings;

namespace Vpiska.Kafka.HostedServices
{
    internal sealed class KafkaProducerHostedService<TEvent> : BackgroundService
    {
        private readonly ILogger<KafkaProducerHostedService<TEvent>> _logger;
        private readonly KafkaProducerSettings<TEvent> _settings;
        private readonly Channel<TEvent> _channel;

        private IProducer<Null, string> _producer;

        public KafkaProducerHostedService(ILogger<KafkaProducerHostedService<TEvent>> logger,
            KafkaProducerSettings<TEvent> settings,
            Channel<TEvent> channel)
        {
            _logger = logger;
            _settings = settings;
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _settings.Servers,
                ClientId = Dns.GetHostName()
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();

            while (!_channel.Reader.Completion.IsCompleted)
            {
                var message = await _channel.Reader.ReadAsync(stoppingToken);
                var data = JsonSerializer.Serialize(message, SerializerOptions.JsonSerializerOptions);
                await _producer.ProduceAsync(_settings.Topic, new Message<Null, string>() { Value = data },
                    stoppingToken);
            }
            
            _producer.Dispose();
        }
    }
}