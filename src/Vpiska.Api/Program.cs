using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Vpiska.Infrastructure;

namespace Vpiska.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddOrleans()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}