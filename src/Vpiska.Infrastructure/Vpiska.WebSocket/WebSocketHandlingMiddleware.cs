using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketHandlingMiddleware<TMessage>
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketsOptions _options;
        private readonly WebSocketHub<TMessage> _hub;

        public WebSocketHandlingMiddleware(RequestDelegate next,
            WebSocketsOptions options,
            IWebSocketSender<TMessage> hub)
        {
            _next = next;
            _options = options;
            _hub = hub as WebSocketHub<TMessage>;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
                
                var socketOptions = _options.UrlOptions[typeof(TMessage)];

                if (!context.Request.Path.StartsWithSegments(socketOptions.Url))
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Can't find path for WebSocket");
                    return;
                }

                var userId = context.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                var username = context.User.Claims.FirstOrDefault(x => x.Type == "Name")?.Value;
                var imageId = context.User.Claims.FirstOrDefault(x => x.Type == "ImageId")?.Value;

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(username))
                {
                    throw new InvalidOperationException("Can't resolve user data from token");
                }

                var queryParams = socketOptions.QueryParams
                    .Where(paramName => context.Request.Query.ContainsKey(paramName))
                    .Select(paramName => new KeyValuePair<string, string>(paramName, context.Request.Query[paramName]))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (queryParams.Count != socketOptions.QueryParams.Count)
                {
                    context.Response.StatusCode = 400;
                    return;
                }
                
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var userContext = new WebSocketUserContext(Guid.Parse(userId), username, imageId);
                var connectionId = _hub.AddConnection(userContext, webSocket, queryParams);
                var buffer = new ArraySegment<byte>(new byte[1024 * 4]);

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            await _hub.ReceiveMessage(connectionId, buffer[..result.Count]);
                            break;
                        case WebSocketMessageType.Close:
                            var isClosed = await _hub.TryCloseConnection(connectionId);
                            if (!isClosed)
                            {
                                throw new InvalidOperationException("Can't close socket");
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Unknown WebSocketMessageType");
                    }
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}