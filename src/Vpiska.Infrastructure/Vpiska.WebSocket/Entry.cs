using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Vpiska.WebSocket
{
    public static class Entry
    {
        public static void AddVSocket<TReceiver, TConnector>(this IServiceCollection services,
            WebSocketsOptions options,
            string url,
            params string[] queryParams)
            where TReceiver : class, IWebSocketReceiver
            where TConnector : class, IWebSocketConnector
        {
            options.AddUrl<TReceiver, TConnector>(url, queryParams);
            services.AddSingleton<TConnector>();
            services.AddSingleton<WebSocketHub<TConnector, TReceiver>>();
            services.AddSingleton<IWebSocketSender<TConnector>, WebSocketSender<TConnector, TReceiver>>();
            services.AddTransient<TReceiver>();
        }

        public static void UseVSocket(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebSocketHandlingMiddleware>();
        }
    }
}