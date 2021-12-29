using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Api.Extensions;
using Vpiska.Api.Filters;
using Vpiska.Jwt;

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
            services.AddSwagger();
            services.AddMongo(_configuration.GetSection("Mongo"));
            services.AddFirebase(_configuration.GetSection("Firebase"));
            services.AddJwt(_configuration.GetSection("Jwt"));
            services.AddValidation();
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
        }
    }
}
