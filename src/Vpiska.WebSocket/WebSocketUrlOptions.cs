using System;
using System.Collections.Generic;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketUrlOptions
    {
        public HashSet<string> IdentityParams { get; }
        
        public HashSet<string> QueryParams { get; }
        
        public Dictionary<string, Func<string>> IdentityParamsDefaultValueGenerators { get; }

        public Type Listener { get; }

        public Type Hub { get; }

        public WebSocketUrlOptions(HashSet<string> identityParams,
            Dictionary<string, Func<string>> identityParamsDefaultValueGenerators,
            HashSet<string> queryParams,
            Type listener,
            Type hub)
        {
            IdentityParams = identityParams;
            IdentityParamsDefaultValueGenerators = identityParamsDefaultValueGenerators;
            QueryParams = queryParams;
            Listener = listener;
            Hub = hub;
        }
    }
}