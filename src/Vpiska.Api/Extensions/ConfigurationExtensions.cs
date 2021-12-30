using System;
using System.Linq;
using System.Reflection;
using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Vpiska.Api.Settings;
using Vpiska.Domain.Models;

namespace Vpiska.Api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
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
        }

        public static void AddFirebase(this IServiceCollection services, IConfigurationSection firebaseSection)
        {
            services.Configure<FirebaseSettings>(firebaseSection);
            var firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase.json")
            });
            var storageClient = StorageClient.Create(GoogleCredential.FromFile("firebase.json"));
            services.AddSingleton(firebaseApp);
            services.AddSingleton(storageClient);
        }

        public static void AddValidation(this IServiceCollection services)
        {
            var validators = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(x => x.GetInterfaces().Any(a => a == typeof(IValidator)))
                .ToArray();

            foreach (var validator in validators)
            {
                var interfaceValidator = validator.GetInterfaces()
                                             .FirstOrDefault(x =>
                                                 x.GetInterfaces().Any(a => a == typeof(IValidator))) ??
                                         throw new InvalidOperationException("Can't find validator");
                services.AddTransient(interfaceValidator, validator);
            }
        }
    }
}