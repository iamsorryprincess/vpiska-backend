using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Vpiska.WebSocket
{
    public static class Entry
    {
        public static void AddWebSocketExceptionHandler<THandler>(this IServiceCollection services) where THandler : class, IWebSocketExceptionHandler
        {
            services.AddTransient<IWebSocketExceptionHandler, THandler>();
        }
        
        public static void AddVSocket<TListener>(this IServiceCollection services,
            string url,
            string[] identityParams,
            string[] queryParams,
            Dictionary<string, Func<string>> identityParamsDefaultValueGenerators = null)
            where TListener : class, IWebSocketListener
        {
            WebSocketHandlingMiddleware.WebSocketsOptions.AddUrl<TListener>(url, identityParams,
                identityParamsDefaultValueGenerators, queryParams);
            services.AddSingleton<WebSocketHub<TListener>>();
            services.AddSingleton<IWebSocketInteracting<TListener>, WebSocketInteracting<TListener>>();
            services.AddTransient<TListener>();
        }

        public static void UseVSocket(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebSocketHandlingMiddleware>();
        }
    }
}