using System;
using System.Collections.Generic;
using System.Linq;

namespace Vpiska.WebSocket
{
    public sealed class WebSocketsOptions
    {
        internal Dictionary<Type, WebSocketUrlOptions> UrlOptions { get; } = new();

        internal void AddUrl<TMessage>(string url, params string[] queryParams)
        {
            if (UrlOptions.Any(x => x.Value.Url == url))
            {
                throw new InvalidOperationException($"Url {url} already added");
            }

            var messageType = typeof(TMessage);

            if (UrlOptions.ContainsKey(messageType))
            {
                throw new InvalidOperationException($"Message type {messageType.FullName} already added");
            }

            UrlOptions.Add(messageType, new WebSocketUrlOptions(url, queryParams.ToHashSet()));
        }
    }
}