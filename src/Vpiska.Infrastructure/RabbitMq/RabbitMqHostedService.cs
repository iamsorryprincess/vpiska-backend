using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
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
    internal sealed class RabbitMqHostedService : IHostedService
    {
        private readonly ILogger<RabbitMqHostedService> _logger;
        private readonly RabbitMqSettings _settings;
        private readonly IEventBus _eventBus;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly RetryPolicy _retryPolicy;
        private readonly List<IDisposable> _subscriptions;
        private readonly List<IModel> _producers;
        private readonly List<IModel> _consumers;

        private IConnection _connection;
        private bool _isConnectionOpened;

        public RabbitMqHostedService(ILogger<RabbitMqHostedService> logger,
            RabbitMqSettings settings,
            IEventBus eventBus,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _settings = settings;
            _eventBus = eventBus;
            _scopeFactory = scopeFactory;
            
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _retryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, _, _) => _logger.LogError(exception, "Trying reconnect to RabbitMQ"));

            _subscriptions = new List<IDisposable>();
            _producers = new List<IModel>();
            _consumers = new List<IModel>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartConnection();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CloseConnection();
            return Task.CompletedTask;
        }

        private void StartConnection()
        {
            _retryPolicy.Execute(() =>
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

            _connection.ConnectionShutdown += OnConnectionShutdown;
            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionBlocked += OnConnectionBlocked;
            _isConnectionOpened = true;
            _logger.LogInformation("Connected to RabbitMQ host {}", _settings.Host);
        }

        private void CloseConnection()
        {
            if (_isConnectionOpened)
            {
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.CallbackException -= OnCallbackException;
                _connection.ConnectionBlocked -= OnConnectionBlocked;
                
                foreach (var consumer in _consumers)
                {
                    consumer?.Close();
                }

                foreach (var producer in _producers)
                {
                    producer?.Close();
                }

                foreach (var subscription in _subscriptions)
                {
                    subscription?.Dispose();
                }
            
                _consumers.Clear();
                _producers.Clear();
                _subscriptions.Clear();
                _connection.Close();
                _isConnectionOpened = false;
            }
        }

        private void SubscribeToEvent<TEvent>(string eventName) where TEvent : class, IDomainEvent, new()
        {
            SubscribeProducer<TEvent>(eventName);
            SubscribeConsumer<TEvent>(eventName);
        }
        
        private void SubscribeProducer<TEvent>(string exchangeName) where TEvent : class, IDomainEvent, new()
        {
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);

            var subscription = _eventBus.EventStream.Subscribe(data =>
            {
                if (data is TEvent domainEvent)
                {
                    var json = JsonSerializer.Serialize(domainEvent, _jsonSerializerOptions);
                    var body = Encoding.UTF8.GetBytes(json);
                    _retryPolicy.Execute(() =>
                        channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: null,
                            body: body));
                }
            });

            _subscriptions.Add(subscription);
            _producers.Add(channel);
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
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
                    var json = Encoding.UTF8.GetString(args.Body.ToArray());
                    var domainEvent = JsonSerializer.Deserialize<TEvent>(json, _jsonSerializerOptions);
                    await handler.Handle(domainEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while consuming event {}", typeof(TEvent).FullName);
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