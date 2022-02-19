using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Api.Filters;
using Vpiska.Domain;
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
            services.AddFirebaseFileStorage(_configuration.GetSection("Firebase"));
            services.AddEventStorage();
            services.AddRabbitMq(_configuration.GetSection("RabbitMQ"));
            services.AddWebSockets();
            services.AddSenders();
            services.AddConnectionsStorages();
            services.AddUserDomain();
            services.AddEventDomain();
            services.AddSwagger();
            services.AddControllers(options => options.Filters.Add<ExceptionFilter>());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "api"));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseWebSockets();
            app.UseVSocket();
        }
    }
}