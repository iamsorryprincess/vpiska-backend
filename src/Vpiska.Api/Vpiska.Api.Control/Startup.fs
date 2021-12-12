namespace Vpiska.Api.Control

open FirebaseAdmin
open Google.Apis.Auth.OAuth2
open Google.Cloud.Storage.V1
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens
open Microsoft.OpenApi.Models
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Conventions
open MongoDB.Driver
open Serilog
open Vpiska.Api.Control.CommandHandlers
open Vpiska.Api.Control.Filters
open Vpiska.Api.Control.Infrastructure.Event
open Vpiska.Api.Control.Infrastructure.Firebase
open Vpiska.Api.Control.Infrastructure.User
open Vpiska.Api.Control.QueryHandlers
open Vpiska.Domain.User
open Vpiska.Infrastructure.Orleans

type Startup(configuration: IConfiguration) =

    static let createTokenParams () =
        TokenValidationParameters(
          ValidateIssuer = true,
          ValidIssuer = Jwt.issuer,
          ValidateAudience = true,
          ValidAudience = Jwt.audience,
          ValidateLifetime = true,
          IssuerSigningKey = Jwt.getKey Jwt.key,
          ValidateIssuerSigningKey = true)
    
    member _.ConfigureServices(services: IServiceCollection) =
        // logger
        let logger = LoggerConfiguration()
                         .WriteTo.Console()
                         .WriteTo.File("logs/log-.txt", rollingInterval = RollingInterval.Day, outputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                         .CreateLogger()
        Log.Logger <- logger
        services.AddSingleton(Log.Logger) |> ignore
        
        // jwt
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options -> options.RequireHttpsMetadata <- false
                                         options.TokenValidationParameters <- createTokenParams()) |> ignore
        
        // swagger  
        services.AddSwaggerGen(fun options ->
            options.SwaggerDoc("v1", OpenApiInfo(Version = "v1", Title = "api"))
            options.AddSecurityDefinition("Bearer",
                                          OpenApiSecurityScheme(In = ParameterLocation.Header,
                                                                Description = "Please insert JWT with Bearer into field",
                                                                Name = "Authorization",
                                                                Type = SecuritySchemeType.ApiKey))
            let req = OpenApiSecurityRequirement()
            req.Add(OpenApiSecurityScheme(Reference = OpenApiReference(Type = ReferenceType.SecurityScheme, Id = "Bearer")), [||])
            options.AddSecurityRequirement(req)) |> ignore
        
        // firebase
        let path = "Infrastructure/Firebase/settings.json"
        let settings = { BucketName = configuration.["Firebase:BucketName"] }
        let firebaseApp = FirebaseApp.Create(AppOptions(Credential = GoogleCredential.FromFile(path)))
        let storageClient = StorageClient.Create(GoogleCredential.FromFile(path))
        services.AddSingleton(settings) |> ignore
        services.AddSingleton(firebaseApp) |> ignore
        services.AddSingleton(storageClient) |> ignore
        
        // mongo
        let conventionPack = ConventionPack()
        CamelCaseElementNameConvention() |> conventionPack.Add
        ImmutableTypeClassMapConvention() |> conventionPack.Add
        ConventionRegistry.Register("default", conventionPack, fun t -> true)
        BsonClassMap.RegisterClassMap<User>(fun cm -> cm.AutoMap(); cm.MapIdMember(fun c -> c.Id) |> ignore) |> ignore
        let settings = { ConnectionString = configuration.["Mongo:ConnectionString"]
                         DatabaseName = configuration.["Mongo:DatabaseName"] }
        let client = MongoClient settings.ConnectionString
        services.AddSingleton(settings) |> ignore
        services.AddSingleton(client) |> ignore
        
        // orleans
        let areas = configuration.GetSection("AreaSettings").GetSection("Areas").AsEnumerable()
                    |> Seq.skip 1
                    |> Seq.map (fun item -> item.Value)
                    |> Seq.toArray
        services.AddSingleton({ Areas = areas }) |> ignore
        services.AddClusterClient(configuration.GetSection("OrleansCluster"))
        services.AddStreamProducer()
        
        // persistence
        services.AddTransient<UserPersistence>() |> ignore
        services.AddTransient<EventCommandsPersistence>() |> ignore
        services.AddTransient<EventQueriesPersistence>() |> ignore
        
        // common
        services.AddControllers(fun options -> options.Filters.Add<ExceptionFilter>() |> ignore) |> ignore

    member _.Configure(app: IApplicationBuilder) =
        app.UseRouting() |> ignore
        app.UseSwagger() |> ignore
        app.UseSwaggerUI(fun c -> c.SwaggerEndpoint("/swagger/v1/swagger.json", "api")) |> ignore
        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore
        app.UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore) |> ignore
        let forwardedHeaders = ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto
        app.UseForwardedHeaders(ForwardedHeadersOptions(ForwardedHeaders = forwardedHeaders)) |> ignore
