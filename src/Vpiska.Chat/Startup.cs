using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Chat.Connectors;
using Vpiska.Chat.Receivers;
using Vpiska.Jwt;
using Vpiska.WebSocket;

namespace Vpiska.Chat
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJwtWebSocket(_configuration.GetSection("Jwt"), "access_token", "/chat");
            var options = new WebSocketsOptions();
            services.AddVSocket<ChatReceiver, ChatConnector>(options, "/chat", new[] { "Id", "Name", "ImageId" },
                new[] { "eventId" }, new Dictionary<string, Func<string>>()
                {
                    { "Id", () => Guid.NewGuid().ToString() }
                });
            services.AddSingleton(options);
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWebSockets();
            app.UseVSocket();
        }
    }
}