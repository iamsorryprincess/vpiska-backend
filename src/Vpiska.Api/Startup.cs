using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Vpiska.Api.Filters;
using Vpiska.Application;
using Vpiska.Application.Event;
using Vpiska.Application.User;
using Vpiska.Infrastructure.Firebase;
using Vpiska.Infrastructure.Jwt;
using Vpiska.Infrastructure.Mongo;
using Vpiska.Infrastructure.Orleans.Grains;

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
            services.AddSingleton(_ => Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day,
                    outputTemplate:
                    "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger());
            
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "api"
                });
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                    In = ParameterLocation.Header, 
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey 
                });
                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { 
                        new OpenApiSecurityScheme 
                        { 
                            Reference = new OpenApiReference 
                            { 
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer" 
                            } 
                        },
                        Array.Empty<string>()
                    } 
                });
            });
            
            services.AddControllers(options => options.Filters.Add<ExceptionFilter>());
            services.AddMongo(_configuration.GetSection("Mongo"));
            services.AddFirebase(_configuration.GetSection("Firebase"));
            services.AddJwt();
            services.AddClusterClient(_configuration.GetSection("OrleansCluster"));
            services.AddPubSubProvider();
            services.AddUserPersistence();
            
            var areas = _configuration.
                GetSection("AreaSettings").GetSection("Areas")
                .AsEnumerable()
                .Skip(1)
                .Select(x => x.Value)
                .ToArray();

            services.AddEventPersistence(areas);
            services.AddSingleton<UserMobileHttpHandler>();
            services.AddSingleton<EventMobileHttpHandler>();
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
