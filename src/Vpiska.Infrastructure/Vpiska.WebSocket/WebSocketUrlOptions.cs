using System;
using System.Collections.Generic;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketUrlOptions
    {
        public HashSet<string> QueryParams { get; }
        
        public Type Connector { get; }
        
        public Type Receiver { get; }
        
        public Type Hub { get; }

        public WebSocketUrlOptions(HashSet<string> queryParams,
            Type connector,
            Type receiver,
            Type hub)
        {
            QueryParams = queryParams;
            Connector = connector;
            Receiver = receiver;
            Hub = hub;
        }
    }
}