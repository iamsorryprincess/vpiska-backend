using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vpiska.Application;
using Vpiska.Infrastructure.Orleans;

namespace Vpiska.ApiChat
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
            services.AddSingleton(_ => Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day,
                    outputTemplate:
                    "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger());
            
            services.AddClusterClient(_configuration.GetSection("OrleansCluster"));

            var areas = _configuration.
                GetSection("AreaSettings").GetSection("Areas")
                .AsEnumerable()
                .Skip(1)
                .Select(x => x.Value)
                .ToArray();

            services.AddEventPersistence(areas);
            services.AddEventChat();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseChatSockets();
        }
    }
}