using System;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Vpiska.Api.Filters;
using Vpiska.Api.Validation.Event;
using Vpiska.Api.Validation.User;
using Vpiska.Domain.EventAggregate.Requests;
using Vpiska.Domain.UserAggregate.RequestHandlers;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Firebase;
using Vpiska.JwtAuthentication;
using Vpiska.Mongo;
using Vpiska.Orleans;

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

            services.AddControllers(options =>
            {
                options.Filters.Add<ExceptionFilter>();
                options.Filters.Add<ValidationFilter>();
            }).AddFluentValidation();
            
            services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
            services.AddScoped<IValidator<CheckCodeRequest>, CheckCodeValidator>();
            services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordValidator>();
            services.AddScoped<IValidator<LoginUserRequest>, LoginUserValidator>();
            services.AddScoped<IValidator<SetCodeRequest>, SetCodeValidator>();
            services.AddScoped<IValidator<Models.UpdateUserRequest>, UpdateUserValidator>();

            services.AddScoped<IValidator<Models.AddMediaRequest>, AddMediaValidator>();
            services.AddScoped<IValidator<Models.CloseEventRequest>, CloseEventValidator>();
            services.AddScoped<IValidator<Models.CreateEventRequest>, CreateEventValidator>();
            services.AddScoped<IValidator<GetEventsRequest>, GetEventsValidator>();
            services.AddScoped<IValidator<GetEventRequest>, GetEventValidator>();
            
            services.AddJwt();
            services.AddFirebase();
            services.AddMongo(_configuration.GetSection("Mongo"));
            services.AddMediatR(typeof(CreateUserHandler));
            services.AddOrleans(_configuration.GetSection("Orleans"));
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