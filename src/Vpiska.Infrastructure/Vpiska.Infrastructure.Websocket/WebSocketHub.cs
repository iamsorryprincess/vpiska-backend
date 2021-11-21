using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Vpiska.Infrastructure.Websocket
{
    internal sealed class WebSocketHub<TConnector, TReceiver> : IWebSocketHub
        where TConnector : IWebSocketConnector
        where TReceiver : IWebSocketReceiver
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebSocketConnector _connector;
        private readonly ConcurrentDictionary<Guid, WebSocketContext> _connections;
        private readonly Type _receiverType;

        public WebSocketHub(IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider)
        {
            _serviceScopeFactory = serviceScopeFactory;
            var connectorType = typeof(TConnector);
            _connector = serviceProvider.GetRequiredService(connectorType) as IWebSocketConnector
                ?? throw new InvalidOperationException($"Can't resolve connector {connectorType.FullName}");
            _connections = new ConcurrentDictionary<Guid, WebSocketContext>();
            _receiverType = typeof(TReceiver);
        }

        public async Task<Guid> AddConnection(WebSocketUserContext userContext, WebSocket webSocket,
            Dictionary<string, string> queryParams)
        {
            var connectionId = Guid.NewGuid();

            if (_connections.TryAdd(connectionId, new WebSocketContext(webSocket, queryParams)))
            {
                await _connector.OnConnect(connectionId, userContext, queryParams);
                return connectionId;
            }

            throw new InvalidOperationException("Can't add webSocket");
        }

        public async Task<bool> TryCloseConnection(Guid connectionId)
        {
            if (_connections.TryRemove(connectionId, out var context))
            {
                await context.Close(WebSocketCloseStatus.NormalClosure, "close connection");
                await _connector.OnDisconnect(connectionId, context.QueryParams);
                return true;
            }

            return false;
        }

        public async Task ReceiveMessage(Guid connectionId, byte[] data)
        {
            try
            {
                if (_connections.TryGetValue(connectionId, out var context))
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var receiver = scope.ServiceProvider.GetRequiredService(_receiverType) as IWebSocketReceiver
                                   ?? throw new InvalidOperationException($"Can't resolve receiver {_receiverType.FullName}");
                    await receiver.Receive(connectionId, data, context.QueryParams);
                }
            }
            catch (Exception)
            {
                if (_connections.TryRemove(connectionId, out var context))
                {
                    await context.Close(WebSocketCloseStatus.InternalServerError, "close connection");
                    await _connector.OnDisconnect(connectionId, context.QueryParams);
                }
            }
        }

        public Task SendMessage(Guid connectionId, byte[] data)
        {
            if (_connections.TryGetValue(connectionId, out var context))
            {
                return context.Send(data);
            }

            throw new InvalidOperationException($"Can't find connection {connectionId}");
        }
    }
}