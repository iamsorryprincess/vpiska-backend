using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Vpiska.WebSocket
{
    public sealed class WebSocketsOptions
    {
        internal Dictionary<PathString, WebSocketUrlOptions> UrlOptions { get; } = new();

        internal void AddUrl<TListener>(string url,
            string[] identityParams,
            Dictionary<string, Func<string>> identityParamsDefaultValueGenerators,
            string[] queryParams) 
            where TListener : IWebSocketListener
        {
            if (UrlOptions.ContainsKey(url))
            {
                throw new InvalidOperationException($"Url {url} already added");
            }

            var listenerType = typeof(TListener);
            
            if (UrlOptions.Any(x => x.Value.Listener == listenerType))
            {
                throw new InvalidOperationException($"Listener {listenerType.FullName} already added");
            }

            UrlOptions.Add(url,
                new WebSocketUrlOptions(identityParams.ToHashSet(),
                    identityParamsDefaultValueGenerators,
                    queryParams.ToHashSet(),
                    listenerType,
                    typeof(WebSocketHub<TListener>)));
        }
    }
}