using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Vpiska.WebSocket
{
    public sealed class WebSocketsOptions
    {
        internal Dictionary<PathString, WebSocketUrlOptions> UrlOptions { get; } = new();

        internal void AddUrl<TReceiver, TConnector>(string url, params string[] queryParams) 
            where TReceiver : IWebSocketReceiver 
            where TConnector : IWebSocketConnector
        {
            if (UrlOptions.ContainsKey(url))
            {
                throw new InvalidOperationException($"Url {url} already added");
            }

            var connectorType = typeof(TConnector);
            if (UrlOptions.Any(x => x.Value.Connector == connectorType))
            {
                throw new InvalidOperationException($"Connector {connectorType.FullName} already added");
            }

            var receiverType = typeof(TReceiver);
            if (UrlOptions.Any(x => x.Value.Receiver == receiverType))
            {
                throw new InvalidOperationException($"Receiver {receiverType.FullName} already added");
            }

            UrlOptions.Add(url,
                new WebSocketUrlOptions(queryParams.ToHashSet(), connectorType, receiverType,
                    typeof(WebSocketHub<TConnector, TReceiver>)));
        }
    }
}