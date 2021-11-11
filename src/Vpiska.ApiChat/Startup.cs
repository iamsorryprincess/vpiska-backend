using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.ApiChat.Connectors;
using Vpiska.ApiChat.Receivers;
using Vpiska.JwtAuthentication;
using Vpiska.WebSocket;

namespace Vpiska.ApiChat
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJwtForWebSocket("access_token", "/chat");
            var options = new WebSocketsOptions();
            services.AddVSocket<ChatReceiver, ChatConnector>(options, "/chat", "eventId");
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