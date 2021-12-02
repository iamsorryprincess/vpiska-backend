using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Vpiska.Infrastructure.Orleans.Interfaces;

namespace Vpiska.OrleansSilo
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
                .UseLocalhostClustering()
                .Configure<EndpointOptions>(a =>
                {
                    a.GatewayPort = 9090;
                    a.SiloPort = 11111;
                    a.AdvertisedIPAddress = IPAddress.Loopback;
                })
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
    }
}