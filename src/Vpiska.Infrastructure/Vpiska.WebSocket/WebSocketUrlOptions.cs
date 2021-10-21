using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Vpiska.WebSocket
{
    internal sealed class WebSocketUrlOptions
    {
        public PathString Url { get; }
        
        public HashSet<string> QueryParams { get; }

        public WebSocketUrlOptions(PathString url, HashSet<string> queryParams)
        {
            Url = url;
            QueryParams = queryParams;
        }
    }
}