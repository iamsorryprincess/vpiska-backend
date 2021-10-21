using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.JwtAuthentication;

namespace Vpiska.ApiChat
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJwtForWebSocket("access_token", "/chat");
            services.AddSingleton<EventStorage>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWebSockets();
        }
    }
}