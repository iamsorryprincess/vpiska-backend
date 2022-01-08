using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Vpiska.Api.Common;
using Vpiska.Api.Filters;
using Vpiska.Api.Models.User;
using Vpiska.Api.Settings;

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
            #region Validation

            var validators = Assembly.GetExecutingAssembly()
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

            #endregion

            #region Jwt

            Identity.Jwt.Audience = _configuration["Jwt:Audience"];
            Identity.Jwt.Issuer = _configuration["Jwt:Issuer"];
            Identity.Jwt.Key = _configuration["Jwt:Key"];
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Identity.Jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = Identity.Jwt.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = Identity.Jwt.GetKey(Identity.Jwt.Key),
                        ValidateIssuerSigningKey = true
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization();

            #endregion

            #region Mongo

            var conventionPack = new ConventionPack()
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("default", conventionPack, _ => true);
            
            BsonClassMap.RegisterClassMap<User>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });

            var mongoClient = new MongoClient(_configuration["Mongo:ConnectionString"]);
            services.AddSingleton<IMongoClient>(mongoClient);
            services.Configure<MongoSettings>(_configuration.GetSection("Mongo"));
            services.AddTransient<DbContext>();

            #endregion

            #region Swagger
            
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

            #endregion

            #region Firebase

            services.Configure<FirebaseSettings>(_configuration.GetSection("Firebase"));
            var firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase.json")
            });
            var storageClient = StorageClient.Create(GoogleCredential.FromFile("firebase.json"));
            services.AddSingleton(firebaseApp);
            services.AddSingleton(storageClient);

            #endregion

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
