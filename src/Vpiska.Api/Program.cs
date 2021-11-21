using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Vpiska.Infrastructure.Orleans;

namespace Vpiska.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .AddClusterClientShutdown()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
