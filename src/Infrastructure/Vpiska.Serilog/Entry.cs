using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Vpiska.Serilog
{
    public static class Entry
    {
        public static void AddSerilog(this IServiceCollection services)
        {
            services.AddSingleton(_ => Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day,
                    outputTemplate:
                    "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger());
        }
    }
}