using System;
using Microsoft.Extensions.Logging;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure.WebSocket
{
    internal sealed class ExceptionHandler : IWebSocketExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public void Handle(WebSocketContext socketContext, Exception exception)
        {
            _logger.LogError(exception, "error while webSocket action");
        }
    }
}