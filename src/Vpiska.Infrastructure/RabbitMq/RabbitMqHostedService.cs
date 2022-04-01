using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Vpiska.Domain.Event.Events.ChatMessageEvent;
using Vpiska.Domain.Event.Events.EventClosedEvent;
using Vpiska.Domain.Event.Events.EventCreatedEvent;
using Vpiska.Domain.Event.Events.EventUpdatedEvent;
using Vpiska.Domain.Event.Events.MediaAddedEvent;
using Vpiska.Domain.Event.Events.MediaRemovedEvent;
using Vpiska.Domain.Event.Events.UserConnectedEvent;
using Vpiska.Domain.Event.Events.UserDisconnectedEvent;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.RabbitMq
{
    internal sealed class RabbitMqHostedService : BackgroundService
    {
        private readonly ILogger<RabbitMqHostedService> _logger;
        private readonly RabbitMqSettings _settings;
        private readonly Channel<IDomainEvent> _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly RetryPolicy _connectionRetryPolicy;
        private readonly RetryPolicy _producerRetryPolicy;
        private readonly Dictionary<string, (string exchangeName, IModel producer)> _producerRegistrations;
        private readonly List<IModel> _consumers;
        private readonly ConcurrentQueue<IDomainEvent> _eventsQueue;

        private IConnection _connection;
        private bool _isConnectionOpened;

        public RabbitMqHostedService(ILogger<RabbitMqHostedService> logger,
            RabbitMqSettings settings,
            Channel<IDomainEvent> channel,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _settings = settings;
            _channel = channel;
            _scopeFactory = scopeFactory;
            
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _connectionRetryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryForever(_ => TimeSpan.FromSeconds(5),
                    (exception, i, _) => _logger.LogError(exception, "Trying reconnect to broker. Try: {}", i));

            _producerRetryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, _, _) => _logger.LogError(exception, "Trying produce message to RabbitMQ"));

            _producerRegistrations = new Dictionary<string, (string exchangeName, IModel producer)>();
            _consumers = new List<IModel>();
            _eventsQueue = new ConcurrentQueue<IDomainEvent>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConnection();
            await foreach (var domainEvent in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                if (_isConnectionOpened)
                {
                    Produce(domainEvent);
                }
                else
                {
                    _eventsQueue.Enqueue(domainEvent);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            CloseConnection();
            return base.StopAsync(cancellationToken);
        }

        private void StartConnection()
        {
            _connectionRetryPolicy.Execute(() =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.Host,
                    UserName = _settings.Username,
                    Password = _settings.Password,
                    DispatchConsumersAsync = true
                };
                _connection = factory.CreateConnection();
            });
            
            SubscribeToEvent<ChatMessageEvent>("event.chat");
            SubscribeToEvent<EventClosedEvent>("event.close");
            SubscribeToEvent<EventCreatedEvent>("event.create");
            SubscribeToEvent<EventUpdatedEvent>("event.update");
            SubscribeToEvent<MediaAddedEvent>("event.media.add");
            SubscribeToEvent<MediaRemovedEvent>("event.media.remove");
            SubscribeToEvent<UserConnectedEvent>("event.user.connect");
            SubscribeToEvent<UserDisconnectedEvent>("event.user.disconnect");

            _isConnectionOpened = true;
            _connection.ConnectionShutdown += OnConnectionShutdown;
            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionBlocked += OnConnectionBlocked;

            while (_eventsQueue.TryDequeue(out var domainEvent))
            {
                Produce(domainEvent);
            }
            
            _logger.LogInformation("Connected to RabbitMQ host {}", _settings.Host);
        }

        private void CloseConnection()
        {
            if (_isConnectionOpened)
            {
                _isConnectionOpened = false;
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.CallbackException -= OnCallbackException;
                _connection.ConnectionBlocked -= OnConnectionBlocked;
                
                foreach (var consumer in _consumers)
                {
                    consumer?.Close();
                }

                foreach (var producerRegistration in _producerRegistrations)
                {
                    producerRegistration.Value.producer?.Close();
                }
            
                _consumers.Clear();
                _producerRegistrations.Clear();
                _connection.Close();
            }
        }
        
        private void SubscribeToEvent<TEvent>(string eventName) where TEvent : class, IDomainEvent, new()
        {
            SubscribeProducer<TEvent>(eventName);
            SubscribeConsumer<TEvent>(eventName);
        }

        private void Produce(IDomainEvent domainEvent)
        {
            if (_producerRegistrations.TryGetValue(domainEvent.GetType().Name, out var tuple))
            {
                var json = JsonSerializer.Serialize(domainEvent as object, _jsonSerializerOptions);
                var body = Encoding.UTF8.GetBytes(json);

                try
                {
                    _producerRetryPolicy.Execute(() => tuple.producer.BasicPublish(
                        exchange: tuple.exchangeName,
                        routingKey: "",
                        basicProperties: null,
                        body: body));
                }
                catch (BrokerUnreachableException)
                {
                    _eventsQueue.Enqueue(domainEvent);
                }
                catch (SocketException)
                {
                    _eventsQueue.Enqueue(domainEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "producing to RabbitMQ: {}", json);
                }
            }
        }
        
        private void SubscribeProducer<TEvent>(string exchangeName) where TEvent : class, IDomainEvent, new()
        {
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);
            _producerRegistrations.Add(typeof(TEvent).Name, (exchangeName, channel));
        }

        private void SubscribeConsumer<TEvent>(string exchangeName, string queueName = null)
            where TEvent : class, IDomainEvent, new()
        {
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);
            
            var queue = queueName == null
                ? channel.QueueDeclare().QueueName
                : channel.QueueDeclare(queue: queueName).QueueName;
            
            channel.QueueBind(exchange: exchangeName, queue: queue, routingKey: "");
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (_, args) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(args.Body.ToArray());
                    var domainEvent = JsonSerializer.Deserialize<TEvent>(json, _jsonSerializerOptions);
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
                    await handler.Handle(domainEvent);
                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while consuming event {}", typeof(TEvent).FullName);
                    channel.BasicReject(args.DeliveryTag, false);
                }
            };

            channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            _consumers.Add(channel);
        }
        
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogError("RabbitMQ connection blocked");
            CloseConnection();
            StartConnection();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError("RabbitMQ callback exception");
            CloseConnection();
            StartConnection();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogError("RabbitMQ connection shutdown");
            CloseConnection();
            StartConnection();
        }
    }
}