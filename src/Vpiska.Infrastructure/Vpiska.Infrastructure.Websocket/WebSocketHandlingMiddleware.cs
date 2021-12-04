using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Vpiska.Infrastructure.Websocket
{
    internal sealed class WebSocketHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketsOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public WebSocketHandlingMiddleware(RequestDelegate next,
            WebSocketsOptions options,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var (url, socketOptions) = _options.UrlOptions
                    .FirstOrDefault(x => context.Request.Path.StartsWithSegments(x.Key));

                if (socketOptions == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Can't find path for WebSocket");
                    return;
                }

                var identityParams = GetIdentityParams(context.User, socketOptions.IdentityParams,
                    socketOptions.IdentityParamsDefaultValueGenerators);

                var queryParams = socketOptions.QueryParams
                    .Where(paramName => context.Request.Query.ContainsKey(paramName))
                    .Select(paramName => new KeyValuePair<string, string>(paramName, context.Request.Query[paramName]))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (queryParams.Count != socketOptions.QueryParams.Count)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Can't find path for WebSocket");
                    return;
                }

                var hub = _serviceProvider.GetRequiredService(socketOptions.Hub) as IWebSocketHub
                          ?? throw new InvalidOperationException($"can't resolve websocket hub for url {url}");
                
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var connectionId = await hub.AddConnection(webSocket, identityParams, queryParams);
                var buffer = new ArraySegment<byte>(new byte[1024 * 4]);

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            await hub.ReceiveMessage(connectionId, buffer[..result.Count].ToArray(), identityParams, queryParams);
                            break;
                        case WebSocketMessageType.Close:
                            await hub.TryCloseConnection(connectionId, identityParams, queryParams);
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

        private static Dictionary<string, string> GetIdentityParams(ClaimsPrincipal user,
            HashSet<string> paramSettings,
            Dictionary<string, Func<string>> defaultValueGenerators)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return paramSettings
                    .Select(paramName =>
                    {
                        if (defaultValueGenerators != null && defaultValueGenerators.ContainsKey(paramName))
                        {
                            return new KeyValuePair<string, string>(paramName,
                                defaultValueGenerators[paramName].Invoke());
                        }

                        return new KeyValuePair<string, string>(paramName, null);
                    })
                    .ToDictionary(x => x.Key, x => x.Value);
            }

            return paramSettings
                .Select(paramName => new KeyValuePair<string, string>(paramName,
                    user.Claims.FirstOrDefault(x => x.Type == paramName)?.Value))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}