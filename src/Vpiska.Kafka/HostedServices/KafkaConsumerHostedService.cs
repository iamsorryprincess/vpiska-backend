using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vpiska.Kafka.Settings;

namespace Vpiska.Kafka.HostedServices
{
    internal sealed class KafkaConsumerHostedService<TEvent> : BackgroundService
    {
        private readonly ILogger<KafkaConsumerHostedService<TEvent>> _logger;
        private readonly KafkaConsumerSettings<TEvent> _settings;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        
        public KafkaConsumerHostedService(ILogger<KafkaConsumerHostedService<TEvent>> logger,
            KafkaConsumerSettings<TEvent> settings,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.Servers,
                GroupId = _settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _settings.Servers }).Build())
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new[]
                    {
                        new TopicSpecification
                        {
                            Name = _settings.Topic,
                            ReplicationFactor = 1,
                            NumPartitions = 1
                        }
                    });
                }
                catch (CreateTopicsException)
                {
                }
            }

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_settings.Topic);

            try
            {
                while (true)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var message = consumer.Consume(stoppingToken);
                    var queueEvent = JsonSerializer.Deserialize<TEvent>(message.Message.Value, SerializerOptions.JsonSerializerOptions);
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var consumerService = scope.ServiceProvider.GetRequiredService<IConsumer<TEvent>>();
                    await consumerService.ConsumeAsync(queueEvent, stoppingToken);
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "consuming listener cancel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error while consuming event");
            }

            consumer.Unsubscribe();
            consumer.Close();
        }
    }
}