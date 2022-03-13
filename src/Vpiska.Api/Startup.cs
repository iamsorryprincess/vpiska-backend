using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Api.Filters;
using Vpiska.Domain;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Media;
using Vpiska.Infrastructure;
using Vpiska.WebSocket;

namespace Vpiska.Api
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
            services.AddMongoDb(_configuration.GetSection("Mongo"));
            services.AddJwt(_configuration.GetSection("Jwt"));
            services.AddFirebaseCloudMessaging();
            services.AddFileStorage();
            services.AddCache<Media>();
            services.AddEventStorage();
            services.AddRabbitMq(_configuration.GetSection("RabbitMQ"));
            services.AddWebSockets();
            services.AddSenders();
            services.AddConnectionsStorages();
            services.AddUserDomain();
            services.AddEventDomain();
            services.AddMediaDomain();
            services.AddSwagger();
            services.AddControllersWithViews(options => options.Filters.Add<ExceptionFilter>());
        }

        public void Configure(IApplicationBuilder app, IEventRepository eventRepository, IEventStorage eventStorage)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "api"));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseWebSockets();
            app.UseVSocket();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var events = eventRepository.GetAll().GetAwaiter().GetResult();
            foreach (var eventData in events)
            {
                eventStorage.SetData(eventData).Wait();
            }
        }
    }
}