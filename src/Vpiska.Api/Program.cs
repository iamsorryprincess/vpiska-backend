using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Vpiska.Api.Orleans;

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
                .UseOrleans((context, siloBuilder) =>
                {
                    var configurationSection = context.Configuration.GetSection("Orleans");
                    
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        siloBuilder.UseLocalhostClustering()
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = configurationSection["ClusterId"];
                                options.ServiceId = configurationSection["ServiceId"];
                            })
                            .AddSimpleMessageStreamProvider("SMSProvider",
                                options => options.OptimizeForImmutableData = false)
                            .AddMemoryGrainStorage("PubSubStore")
                            .ConfigureApplicationParts(parts =>
                                parts.AddApplicationPart(typeof(IEventGrain).Assembly).WithReferences());
                    }
                    else
                    {
                        var redisConnectionString =
                            $"{configurationSection["Redis:Host"]}:{configurationSection["Redis:Port"]}";

                        siloBuilder.UseRedisClustering(configuration =>
                            {
                                configuration.ConnectionString = redisConnectionString;
                                configuration.Database = 0;
                            })
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = configurationSection["ClusterId"];
                                options.ServiceId = configurationSection["ServiceId"];
                            })
                            .ConfigureEndpoints(11111, 30000, listenOnAnyHostAddress: true)
                            .AddSimpleMessageStreamProvider("SMSProvider",
                                options => options.OptimizeForImmutableData = false)
                            .AddRedisGrainStorage("PubSubStore", optionsBuilder => optionsBuilder.Configure(
                                configuration =>
                                {
                                    configuration.ConnectionString = redisConnectionString;
                                    configuration.UseJson = true;
                                    configuration.DatabaseNumber = 1;
                                }))
                            .ConfigureApplicationParts(parts =>
                                parts.AddApplicationPart(typeof(IEventGrain).Assembly).WithReferences());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
