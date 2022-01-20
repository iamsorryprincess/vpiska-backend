using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Infrastructure.Database;
using Vpiska.Infrastructure.Firebase;
using Vpiska.Infrastructure.Identity;
using Vpiska.Infrastructure.Orleans;
using Vpiska.Infrastructure.WebSocket;
using Vpiska.WebSocket;

namespace Vpiska.Infrastructure
{
    public static class Entry
    {
        #region MongoDB
        
        public static void AddMongoDb(this IServiceCollection services, IConfigurationSection mongoSection)
        {
            var conventionPack = new ConventionPack()
            {
                new CamelCaseElementNameConvention(),
                new ImmutableTypeClassMapConvention()
            };
            ConventionRegistry.Register("default", conventionPack, _ => true);
            
            services.AddMongoDbUserConfiguration();
            services.AddMongoDbEventConfiguration();
            var client = new MongoClient(mongoSection["ConnectionString"]);
            var settings = new MongoSettings(mongoSection["DatabaseName"]);
            services.AddSingleton<IMongoClient>(client);
            services.AddSingleton(settings);
        }

        private static void AddMongoDbUserConfiguration(this IServiceCollection services)
        {
            BsonClassMap.RegisterClassMap<User>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });

            services.AddTransient<IUserRepository, UserRepository>();
        }

        private static void AddMongoDbEventConfiguration(this IServiceCollection services)
        {
            BsonClassMap.RegisterClassMap<Event>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });

            services.AddTransient<IEventRepository, EventRepository>();
        }
        
        #endregion

        #region Identity

        public static void AddJwt(this IServiceCollection services, IConfigurationSection jwtSection)
        {
            var settings = new IdentitySettings(jwtSection["Key"],
                jwtSection["Issuer"],
                jwtSection["Audience"],
                int.Parse(jwtSection["LifetimeDays"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = settings.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = settings.GetKey(),
                        ValidateIssuerSigningKey = true
                    };
                    options.AddJwtForWebSocket("access_token", "/chat");
                });
            services.AddAuthorization();
            services.AddSingleton(settings);
            services.AddSingleton<IIdentityService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
        }

        private static void AddJwtForWebSocket(this JwtBearerOptions options, string queryParam, string route)
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query[queryParam];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(route))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        }
        
        #endregion

        #region Swagger

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

        #endregion

        #region Firebase

        public static void AddFirebaseCloudMessaging(this IServiceCollection services)
        {
            var firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(GetSettingsPath())
            });
            services.AddSingleton(firebaseApp);
            services.AddSingleton<INotificationService, CloudMessagingService>();
        }

        public static void AddFirebaseFileStorage(this IServiceCollection services, IConfigurationSection firebaseSection)
        {
            var settings = new FirebaseSettings(firebaseSection["BucketName"]);
            services.AddSingleton(settings);
            var storageClient = StorageClient.Create(GoogleCredential.FromFile(GetSettingsPath()));
            services.AddSingleton(storageClient);
            services.AddTransient<IFileStorage, FileStorage>();
        }

        private static string GetSettingsPath()
        {
#if DEBUG
            const string path = "../Vpiska.Infrastructure/Firebase/settings.json";
#else
            const string path = "Firebase/settings.json";
#endif
            return path;
        }

        #endregion

        #region Orleans
        
        public static void AddEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<EventHandlersResolver>();
        }

        public static void AddEventCache(this IServiceCollection services)
        {
            services.AddTransient<ICache<Event>, EventsCache>();
        }

        public static void AddEventStateManager(this IServiceCollection services)
        {
            services.AddTransient<IEventStateManager, EventStateManager>();
        }

        public static IHostBuilder AddOrleans(this IHostBuilder builder)
        {
            return builder.UseOrleans((context, siloBuilder) =>
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
                        .AddSimpleMessageStreamProvider(Constants.StreamMessageProvider,
                            options => options.OptimizeForImmutableData = false)
                        .AddMemoryGrainStorage(Constants.StorageProvider)
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
                        .AddSimpleMessageStreamProvider(Constants.StreamMessageProvider,
                            options => options.OptimizeForImmutableData = false)
                        .AddRedisGrainStorage(Constants.StorageProvider, optionsBuilder => optionsBuilder.Configure(
                            configuration =>
                            {
                                configuration.ConnectionString = redisConnectionString;
                                configuration.UseJson = true;
                                configuration.DatabaseNumber = 1;
                            }))
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(IEventGrain).Assembly).WithReferences());
                }
            });
        }

        #endregion

        #region WebSocket

        public static void AddEventSender(this IServiceCollection services)
        {
            services.AddTransient<IEventSender, EventSender>();
        }

        public static void AddConnectionsStorage(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionsStorage, Storage>();
        }

        public static void AddWebSockets(this IServiceCollection services)
        {
            var options = new WebSocketsOptions();
            var idGenerators = new Dictionary<string, Func<string>> { { "Id", () => Guid.NewGuid().ToString() } };
            services.AddVSocket<ChatReceiver, ChatConnector>(options, "/event", new[] { "Id", "Name", "ImageId" },
                new[] { "eventId" }, idGenerators);
            services.AddSingleton(options);
        }

        #endregion
    }
}