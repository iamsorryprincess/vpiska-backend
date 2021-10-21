using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Vpiska.Api.Filters;
using Vpiska.Api.Validation;
using Vpiska.Domain.UserAggregate.RequestHandlers;
using Vpiska.Domain.UserAggregate.Requests;
using Vpiska.Firebase;
using Vpiska.JwtAuthentication;
using Vpiska.Mongo;

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
            });

            services.AddControllers(options =>
            {
                options.Filters.Add<ExceptionFilter>();
                options.Filters.Add<ValidationFilter>();
            }).AddFluentValidation();
            
            services.AddTransient<IValidator<CreateUserRequest>, CreateUserValidator>();
            services.AddTransient<IValidator<CheckCodeRequest>, CheckCodeValidator>();
            services.AddTransient<IValidator<ChangePasswordRequest>, ChangePasswordValidator>();
            services.AddTransient<IValidator<LoginUserRequest>, LoginUserValidator>();
            services.AddTransient<IValidator<SetCodeRequest>, SetCodeValidator>();
            services.AddTransient<IValidator<Models.UpdateUserRequest>, UpdateUserValidator>();
            
            services.AddJwt();
            services.AddFirebase();
            services.AddMongo(_configuration.GetSection("Mongo"));
            services.AddMediatR(typeof(CreateUserHandler));
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