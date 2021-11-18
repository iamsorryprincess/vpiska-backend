using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Vpiska.Infrastructure.Orleans.Grains.Interfaces;

namespace Vpiska.Infrastructure.Orleans.Grains
{
    public static class Entry
    {
        public static void AddPubSubProvider(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IOrleansPubSubProvider<>), typeof(OrleansPubSubProvider<>));
        }

        public static void AddEventConsumer<TEventData, TConsumer>(this IServiceCollection services)
            where TConsumer : class, IEventConsumer<TEventData>
        {
            services.AddTransient<IEventConsumer<TEventData>, TConsumer>();
        }

        public static void AddClusterClient(this IServiceCollection services, IConfigurationSection clusterSection)
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterSection["ClusterId"];
                    options.ServiceId = clusterSection["ServiceId"];
                })
                .Build();
            client.Connect().Wait();
            services.AddSingleton(client);
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