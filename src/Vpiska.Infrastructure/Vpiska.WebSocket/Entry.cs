using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Vpiska.WebSocket
{
    public static class Entry
    {
        public static void AddVSocket<TMessage, TReceiver, TConnector>(this IServiceCollection services,
            WebSocketsOptions options,
            string url,
            params string[] queryParams)
            where TReceiver : class, IWebSocketReceiver<TMessage>
            where TConnector : class, IWebSocketConnector<TMessage>
        {
            options.AddUrl<TMessage>(url, queryParams);
            services.AddSingleton<IWebSocketConnector<TMessage>, TConnector>();
            services.AddSingleton<IWebSocketSender<TMessage>, WebSocketHub<TMessage>>();
            services.AddTransient<IWebSocketReceiver<TMessage>, TReceiver>();
        }

        public static void UseVSocket<TMessage>(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebSocketHandlingMiddleware<TMessage>>();
        }
    }
}