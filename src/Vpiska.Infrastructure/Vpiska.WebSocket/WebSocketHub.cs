using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketHub<TMessage> : IWebSocketSender<TMessage>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebSocketConnector<TMessage> _connector;
        private readonly ConcurrentDictionary<Guid, WebSocketContext> _connections;

        public WebSocketHub(IServiceScopeFactory serviceScopeFactory, IWebSocketConnector<TMessage> connector)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _connector = connector;
            _connections = new ConcurrentDictionary<Guid, WebSocketContext>();
        }

        public Guid AddConnection(WebSocketUserContext userContext, System.Net.WebSockets.WebSocket webSocket,
            Dictionary<string, string> queryParams)
        {
            var connectionId = Guid.NewGuid();

            if (_connections.TryAdd(connectionId, new WebSocketContext(webSocket, queryParams)))
            {
                _connector.OnConnect(connectionId, userContext, queryParams);
                return connectionId;
            }

            throw new InvalidOperationException("Can't add webSocket");
        }

        public async Task<bool> TryCloseConnection(Guid connectionId)
        {
            if (_connections.TryRemove(connectionId, out var context))
            {
                await context.Close(WebSocketCloseStatus.NormalClosure, "close connection");
                _connector.OnDisconnect(connectionId, context.QueryParams);
                return true;
            }

            return false;
        }

        public async Task ReceiveMessage(Guid connectionId, ArraySegment<byte> data)
        {
            try
            {
                if (_connections.TryGetValue(connectionId, out var context))
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var message = JsonSerializer.Deserialize<TMessage>(data, _jsonOptions);
                    var receiver = scope.ServiceProvider.GetRequiredService<IWebSocketReceiver<TMessage>>();
                    await receiver.Receive(connectionId, message, context.QueryParams);
                }
            }
            catch (Exception)
            {
                if (_connections.TryRemove(connectionId, out var context))
                {
                    await context.Close(WebSocketCloseStatus.InternalServerError, "close connection");
                    _connector.OnDisconnect(connectionId, context.QueryParams);
                }
            }
        }

        public Task SendMessage(Guid connectionId, TMessage message)
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var data = Encoding.UTF8.GetBytes(json);
            
            if (_connections.TryGetValue(connectionId, out var context))
            {
                return context.Send(data);
            }

            throw new InvalidOperationException($"Can't find connection {connectionId}");
        }

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}