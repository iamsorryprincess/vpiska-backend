using System;
using System.Collections.Generic;

namespace Vpiska.Infrastructure.Websocket
{
    internal sealed class WebSocketUrlOptions
    {
        public HashSet<string> IdentityParams { get; }
        
        public HashSet<string> QueryParams { get; }

        public Type Connector { get; }
        
        public Type Receiver { get; }
        
        public Type Hub { get; }

        public WebSocketUrlOptions(HashSet<string> identityParams,
            HashSet<string> queryParams,
            Type connector,
            Type receiver,
            Type hub)
        {
            IdentityParams = identityParams;
            QueryParams = queryParams;
            Connector = connector;
            Receiver = receiver;
            Hub = hub;
        }
    }
}