using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Vpiska.Infrastructure.Orleans.Interfaces;
using Vpiska.Infrastructure.Orleans.Streaming;

namespace Vpiska.Infrastructure.Orleans
{
    public static class Entry
    {
        public static void AddClusterClient(this IServiceCollection services, IConfigurationSection clusterSection)
        {
            var client = new ClientBuilder()
                .UseStaticClustering(opt =>
                {
                    opt.Gateways.Add(new Uri("gwy.tcp://127.0.0.1:9090/0"));
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterSection["ClusterId"];
                    options.ServiceId = clusterSection["ServiceId"];
                })
                .AddSimpleMessageStreamProvider("chatProvider")
                .Build();
            client.Connect().Wait();
            services.AddSingleton(client);
        }

        public static void AddStreamProducer(this IServiceCollection services)
        {
            services.AddSingleton<IStreamProducer, StreamProducer>();
        }

        public static void AddStreamConsumer<TConsumer>(this IServiceCollection services) where TConsumer : class, IStreamConsumer
        {
            services.AddTransient<IStreamConsumer, TConsumer>();
        }

        public static IHost AddClusterClientShutdown(this IHost host)
        {
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopped.Register(() =>
            {
                var clusterClient = host.Services.GetRequiredService<IClusterClient>();
                clusterClient.Close().Wait();
            });
            return host;
        }
    }
}