using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media;
using Vpiska.Domain.Media.Interfaces;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Infrastructure.Cache;
using Vpiska.Infrastructure.Database;
using Vpiska.Infrastructure.Firebase;
using Vpiska.Infrastructure.Identity;
using Vpiska.Infrastructure.RabbitMq;
using Vpiska.Infrastructure.WebSocket;
using Vpiska.WebSocket;
using Constants = Vpiska.Domain.Media.Constants;

namespace Vpiska.Infrastructure
{
    public static class Entry
    {
        #region MongoDB
        
        public static void AddMongoDb(this IServiceCollection services, IConfigurationSection mongoSection)
        {
            var conventionPack = new ConventionPack()
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("default", conventionPack, _ => true);
            
            services.AddMongoDbUserConfiguration();
            services.AddMongoDbEventConfiguration();
            services.AddMongoDbMediaConfigurations();
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

        private static void AddMongoDbMediaConfigurations(this IServiceCollection services)
        {
            BsonClassMap.RegisterClassMap<Media>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });

            services.AddTransient<IMediaRepository, MediaRepository>();
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
                    options.AddJwtForWebSocket("access_token", "/websockets/event");
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
            services.AddTransient<IFileStorage, Firebase.FileStorage>();
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

        #region Cache

        public static void AddCache<T>(this IServiceCollection services) where T : class, new()
        {
            services.AddSingleton<ICache<T>, Cache<T>>();
        }

        #endregion
        
        #region FileStorage

        public static void AddFileStorage(this IServiceCollection services)
        {
            if (!Directory.Exists(Constants.Path))
            {
                Directory.CreateDirectory(Constants.Path);
            }
            services.AddTransient<IFileStorage, FileStorage.FileStorage>();
        }

        #endregion

        #region EventStorage

        public static void AddEventStorage(this IServiceCollection services)
        {
            services.AddSingleton<IEventStorage, EventState.EventStorage>();
        }

        #endregion

        #region RabbitMQ

        public static void AddRabbitMq(this IServiceCollection services, IConfigurationSection rabbitConfiguration)
        {
            var settings = new RabbitMqSettings()
            {
                Host = rabbitConfiguration["Host"],
                Username = rabbitConfiguration["User"],
                Password = rabbitConfiguration["Password"]
            };
            services.AddSingleton(settings);
            services.AddSingleton(Channel.CreateUnbounded<IDomainEvent>(new UnboundedChannelOptions()
            {
                SingleReader = true
            }));
            services.AddSingleton<IEventBus, EventBus>();
            services.AddHostedService<RabbitMqHostedService>();
        }

        #endregion
        
        #region WebSocket

        public static void AddSenders(this IServiceCollection services)
        {
            services.AddTransient<IEventSender, EventSender>();
            services.AddTransient<IUserSender, UserSender>();
        }

        public static void AddConnectionsStorages(this IServiceCollection services)
        {
            services.AddSingleton<IEventConnectionsStorage, EventConnectionsStorage>();
            services.AddSingleton<IUserConnectionsStorage, UserConnectionsStorage>();
        }

        public static void AddWebSockets(this IServiceCollection services)
        {
            services.AddWebSocketExceptionHandler<ExceptionHandler>();
            
            var idGenerators = new Dictionary<string, Func<string>> { { "Id", () => Guid.NewGuid().ToString() } };

            services.AddVSocket<ChatListener>("/websockets/event",
                new[] { "Id", "Name", "ImageId" },
                new[] { "eventId" },
                idGenerators);

            services.AddVSocket<RangeListener>("/websockets/range",
                Array.Empty<string>(),
                Array.Empty<string>());
        }

        #endregion
    }
}