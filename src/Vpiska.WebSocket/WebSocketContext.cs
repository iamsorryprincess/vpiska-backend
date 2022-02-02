using System;
using System.Collections.Generic;

namespace Vpiska.WebSocket
{
    public sealed class WebSocketContext
    {
        public Guid ConnectionId { get; }

        public Dictionary<string, string> QueryParams { get; }

        public Dictionary<string, string> IdentityParams { get; }

        public IServiceProvider ServiceProvider { get; }

        internal WebSocketContext(Guid connectionId,
            Dictionary<string, string> queryParams,
            Dictionary<string, string> identityParams,
            IServiceProvider serviceProvider)
        {
            ConnectionId = connectionId;
            QueryParams = queryParams;
            IdentityParams = identityParams;
            ServiceProvider = serviceProvider;
        }
    }
}