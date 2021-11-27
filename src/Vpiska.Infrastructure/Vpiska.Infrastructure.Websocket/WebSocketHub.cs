using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
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
        private readonly ConcurrentDictionary<Guid, WebSocket> _connections;
        private readonly Type _receiverType;

        public WebSocketHub(IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider)
        {
            _serviceScopeFactory = serviceScopeFactory;
            var connectorType = typeof(TConnector);
            _connector = serviceProvider.GetRequiredService(connectorType) as IWebSocketConnector
                ?? throw new InvalidOperationException($"Can't resolve connector {connectorType.FullName}");
            _connections = new ConcurrentDictionary<Guid, WebSocket>();
            _receiverType = typeof(TReceiver);
        }

        public async Task<Guid> AddConnection(WebSocket webSocket, Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams)
        {
            var connectionId = Guid.NewGuid();

            if (_connections.TryAdd(connectionId, webSocket))
            {
                await _connector.OnConnect(connectionId, identityParams, queryParams);
                return connectionId;
            }

            throw new InvalidOperationException("Can't add webSocket");
        }

        public async Task<bool> TryCloseConnection(Guid connectionId, Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams)
        {
            if (_connections.TryRemove(connectionId, out var socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close connection", CancellationToken.None);
                await _connector.OnDisconnect(connectionId, identityParams, queryParams);
                return true;
            }

            return false;
        }

        public async Task ReceiveMessage(Guid connectionId, byte[] data,
            Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams)
        {
            try
            {
                if (_connections.ContainsKey(connectionId))
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var receiver = scope.ServiceProvider.GetRequiredService(_receiverType) as IWebSocketReceiver
                                   ?? throw new InvalidOperationException($"Can't resolve receiver {_receiverType.FullName}");
                    await receiver.Receive(connectionId, data, identityParams, queryParams);
                }
            }
            catch (Exception)
            {
                if (_connections.TryRemove(connectionId, out var socket))
                {
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "close connection", CancellationToken.None);
                    await _connector.OnDisconnect(connectionId, identityParams, queryParams);
                }
            }
        }

        public Task SendMessage(Guid connectionId, byte[] data)
        {
            if (_connections.TryGetValue(connectionId, out var socket))
            {
                return socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }

            throw new InvalidOperationException($"Can't find connection {connectionId}");
        }

        public Task Close(Guid connectionId)
        {
            if (_connections.TryRemove(connectionId, out var socket))
            {
                return socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close connection", CancellationToken.None);
            }

            throw new InvalidOperationException($"Can't find connection {connectionId}");
        }
    }
}