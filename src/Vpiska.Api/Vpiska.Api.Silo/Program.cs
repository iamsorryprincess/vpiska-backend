using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
            
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var builder = new SiloHostBuilder()
                .ConfigureEndpoints(11111, 30000)
                .AddClustering(config.GetSection("OrleansCluster").GetSection("Redis"))
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = config["OrleansCluster:ClusterId"];
                    options.ServiceId = config["OrleansCluster:ServiceId"];
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(IEventGrain).Assembly).WithReferences())
                .AddSimpleMessageStreamProvider("chatProvider", (options) => options.OptimizeForImmutableData = false)
                .AddGrainStorage(config.GetSection("OrleansCluster").GetSection("Redis"))
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

        private static ISiloHostBuilder AddClustering(this ISiloHostBuilder builder, IConfigurationSection redisSection)
        {
#if DEBUG
            var result = builder.UseLocalhostClustering();
#else
            var result = builder.UseRedisClustering(options =>
            {
                options.ConnectionString = $"{redisSection["Host"]}:{redisSection["Port"]}";
                options.Database = 0;
            });
#endif
            return result;
        }

        private static ISiloHostBuilder AddGrainStorage(this ISiloHostBuilder builder, IConfigurationSection redisSection)
        {
            return builder
#if DEBUG
                .AddMemoryGrainStorage("PubSubStore");
#else
                .AddRedisGrainStorage("Redis", optionsBuilder => optionsBuilder.Configure(options =>
                {
                    options.ConnectionString = $"{redisSection["Host"]}:{redisSection["Port"]}";
                    options.UseJson = true;
                    options.DatabaseNumber = 1;
                }));
#endif
        }
    }
}