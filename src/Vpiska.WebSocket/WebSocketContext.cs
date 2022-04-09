using System;
using System.Collections.Generic;

namespace Vpiska.WebSocket
{
    public sealed class WebSocketContext
    {
        public Guid ConnectionId { get; }

        public Guid[] Connections { get; }

        public Dictionary<string, string> QueryParams { get; }

        public Dictionary<string, string> IdentityParams { get; }

        public IServiceProvider ServiceProvider { get; }

        internal WebSocketContext(Guid connectionId,
            Guid[] connections,
            Dictionary<string, string> queryParams,
            Dictionary<string, string> identityParams,
            IServiceProvider serviceProvider)
        {
            ConnectionId = connectionId;
            Connections = connections;
            QueryParams = queryParams;
            IdentityParams = identityParams;
            ServiceProvider = serviceProvider;
        }
    }
}