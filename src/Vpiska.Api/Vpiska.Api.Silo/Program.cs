using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Vpiska.Infrastructure.Orleans.Interfaces;

namespace Vpiska.Api.Silo
{
    public static class Program
    {
        private static readonly object SyncLock = new();
        private static readonly ManualResetEvent SiloStopped = new(false);
        
        private static ISiloHost _silo;
        private static bool _siloStopping;

        private static void Main(string[] args)
        {
            SetupApplicationShutdown();

            var builder = new SiloHostBuilder()
                .ConfigureEndpoints(11111, 30000)
                .AddClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(IEventGrain).Assembly).WithReferences())
                .AddSimpleMessageStreamProvider("chatProvider", (options) => options.OptimizeForImmutableData = false)
                .AddMemoryGrainStorage("PubSubStore")
                .ConfigureLogging(logging => logging.AddConsole());

            _silo = builder.Build();
            _silo.StartAsync().Wait();
            SiloStopped.WaitOne();
        }

        private static void SetupApplicationShutdown()
        {
            Console.CancelKeyPress += (s, a) =>
            {
                a.Cancel = true;
                lock (SyncLock)
                {
                    if (_siloStopping) return;
                    _siloStopping = true;
                    Task.Run(StopSilo).Ignore();
                }
            };
        }

        private static async Task StopSilo()
        {
            await _silo.StopAsync();
            SiloStopped.Set();
        }

        private static ISiloHostBuilder AddClustering(this ISiloHostBuilder builder)
        {
#if DEBUG
            var result = builder.UseLocalhostClustering();
#else
            var result = builder.UseRedisClustering(options =>
            {
                options.ConnectionString = "redis:6379";
                options.Database = 0;
            });
#endif
            return result;
        }
    }
}