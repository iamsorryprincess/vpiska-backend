using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketHub<TListener> : IWebSocketHub where TListener : IWebSocketListener
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConcurrentDictionary<Guid, System.Net.WebSockets.WebSocket> _connections;
        private readonly Type _listenerType;

        public WebSocketHub(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _connections = new ConcurrentDictionary<Guid, System.Net.WebSockets.WebSocket>();
            _listenerType = typeof(TListener);
        }

        public async Task<Guid> AddConnection(System.Net.WebSockets.WebSocket webSocket,
            Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams)
        {
            var connectionId = Guid.NewGuid();
            
            if (_connections.TryAdd(connectionId, webSocket))
            {
                try
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var listener = scope.ServiceProvider.GetRequiredService(_listenerType) as IWebSocketListener
                                   ?? throw new InvalidOperationException(
                                       $"Can't resolve listener {_listenerType.FullName}");
                    await listener.OnConnect(new WebSocketContext(connectionId, queryParams, identityParams,
                        scope.ServiceProvider));
                    return connectionId;
                }
                catch (Exception ex)
                {
                    HandleException(connectionId, identityParams, queryParams, ex);
                    await CloseConnection(webSocket, "error while trying create connection", WebSocketCloseStatus.InternalServerError,
                        connectionId,
                        identityParams,
                        queryParams);
                }
            }
            
            throw new InvalidOperationException("Can't add webSocket");
        }

        public async Task<bool> TryCloseConnection(Guid connectionId, Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams)
        {
            if (_connections.TryRemove(connectionId, out var socket))
            {
                var result = await CloseConnection(socket, "close connection", WebSocketCloseStatus.NormalClosure,
                    connectionId,
                    identityParams,
                    queryParams);
                return result;
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
                    var strData = Encoding.UTF8.GetString(data);
                    var splitIndex = strData.IndexOf('/');
                    var route = strData[..splitIndex];
                    var message = strData[(splitIndex + 1)..];
                    var listener = scope.ServiceProvider.GetRequiredService(_listenerType) as IWebSocketListener
                                   ?? throw new InvalidOperationException(
                                       $"Can't resolve listener {_listenerType.FullName}");
                    await listener.Receive(
                        new WebSocketContext(connectionId, queryParams, identityParams, scope.ServiceProvider), route,
                        message);
                }
            }
            catch (Exception ex)
            {
                HandleException(connectionId, identityParams, queryParams, ex);
                
                if (_connections.TryRemove(connectionId, out var socket))
                {
                    await CloseConnection(socket, "error while receive message",
                        WebSocketCloseStatus.InternalServerError,
                        connectionId,
                        identityParams,
                        queryParams);
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
                return CloseSafeAsync(socket, WebSocketCloseStatus.NormalClosure, "close connection",
                    CancellationToken.None);
            }

            throw new InvalidOperationException($"Can't find connection {connectionId}");
        }
        
        private async Task<bool> CloseConnection(System.Net.WebSockets.WebSocket socket,
            string statusDescription,
            WebSocketCloseStatus socketCloseStatus,
            Guid connectionId,
            Dictionary<string, string> identityParams,
            Dictionary<string, string> queryParams)
        {
            try
            {
                await CloseSafeAsync(socket, socketCloseStatus, statusDescription, CancellationToken.None);
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var listener = scope.ServiceProvider.GetRequiredService(_listenerType) as IWebSocketListener
                               ?? throw new InvalidOperationException(
                                   $"Can't resolve listener {_listenerType.FullName}");
                await listener.OnDisconnect(new WebSocketContext(connectionId, queryParams, identityParams,
                    scope.ServiceProvider));
                return true;
            }
            catch (Exception ex)
            {
                HandleException(connectionId, identityParams, queryParams, ex);
                return false;
            }
        }

        private void HandleException(Guid connectionId, Dictionary<string, string> identityParams, Dictionary<string, string> queryParams, Exception ex)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var exceptionHandler = scope.ServiceProvider.GetService<IWebSocketExceptionHandler>();
            exceptionHandler?.Handle(
                new WebSocketContext(connectionId, queryParams, identityParams, scope.ServiceProvider), ex);
        }
        
        private static async Task CloseSafeAsync(System.Net.WebSockets.WebSocket webSocket,
            WebSocketCloseStatus closeStatus,
            string statusDescription,
            CancellationToken cancellationToken)
        {
            if (webSocket.State == WebSocketState.Aborted)
            {
                webSocket.Dispose();
                return;
            }

            await webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }
    }
}