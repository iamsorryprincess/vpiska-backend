namespace Vpiska.Application

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open FirebaseAdmin
open Google.Apis.Auth.OAuth2
open Google.Cloud.Storage.V1
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Conventions
open MongoDB.Driver
open Vpiska.Application.Event.WebSocket
open Vpiska.Application.Firebase
open Vpiska.Application.User
open Vpiska.Domain.User
open Vpiska.Infrastructure.Websocket
open Vpiska.Infrastructure.Orleans

[<Extension>]
type Entry() =
    
    static let createTokenParams () =
        TokenValidationParameters(
          ValidateIssuer = true,
          ValidIssuer = Jwt.issuer,
          ValidateAudience = true,
          ValidAudience = Jwt.audience,
          ValidateLifetime = true,
          IssuerSigningKey = Jwt.getKey Jwt.key,
          ValidateIssuerSigningKey = true)
        
    static let handle (context: MessageReceivedContext) (tokenQueryParam: string) (webSocketUrl: string) =
        match context.Request.Query.Count with
        | 0 -> Task.CompletedTask
        | _ ->
            let accessToken = context.Request.Query.[tokenQueryParam].Item 0
            let path = context.HttpContext.Request.Path
            if String.IsNullOrWhiteSpace(accessToken) |> not && path.StartsWithSegments(PathString(webSocketUrl)) then
                context.Token <- accessToken
            Task.CompletedTask
    
    [<Extension>]
    static member AddJwt(services: IServiceCollection) =
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options -> options.RequireHttpsMetadata <- false
                                         options.TokenValidationParameters <- createTokenParams()) |> ignore
    
    [<Extension>]   
    static member AddJwtForWebsocket(services: IServiceCollection, tokenQueryParam: string, webSocketUrl: string) =
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options -> options.RequireHttpsMetadata <- false
                                         options.TokenValidationParameters <- createTokenParams()
                                         options.Events <- JwtBearerEvents(OnMessageReceived =
                                             Func<MessageReceivedContext, Task>(fun context -> handle context tokenQueryParam webSocketUrl))) |> ignore
        services.AddAuthorization() |> ignore
    
    [<Extension>]
    static member AddFirebase(services: IServiceCollection, firebaseSection: IConfigurationSection) =
        #if DEBUG
        let path = "../Vpiska.Application/Firebase/settings.json"
        #else
        let path = "Firebase/settings.json"
        #endif
        let settings = { BucketName = firebaseSection.["BucketName"] }: Storage.FirebaseSettings
        let firebaseApp = FirebaseApp.Create(AppOptions(Credential = GoogleCredential.FromFile(path)))
        let storageClient = StorageClient.Create(GoogleCredential.FromFile(path))
        services.AddSingleton(settings) |> ignore
        services.AddSingleton(firebaseApp) |> ignore
        services.AddSingleton(storageClient) |> ignore
    
    [<Extension>]
    static member AddMongo(services: IServiceCollection, mongoSection: IConfigurationSection) =
        let conventionPack = ConventionPack()
        CamelCaseElementNameConvention() |> conventionPack.Add
        ImmutableTypeClassMapConvention() |> conventionPack.Add
        ConventionRegistry.Register("default", conventionPack, fun t -> true)
        BsonClassMap.RegisterClassMap<User>(fun cm -> cm.AutoMap(); cm.MapIdMember(fun c -> c.Id) |> ignore) |> ignore
        let settings = { ConnectionString = mongoSection.["ConnectionString"]
                         DatabaseName = mongoSection.["DatabaseName"] }: Database.MongoSettings
        let client = MongoClient settings.ConnectionString
        services.AddSingleton(settings) |> ignore
        services.AddSingleton(client) |> ignore
    
    [<Extension>]
    static member AddEventChat(services: IServiceCollection) =
        services.AddStreamConsumer<ChatConsumer>()
        services.AddJwtForWebsocket("access_token", "/chat")
        let options = WebSocketsOptions()
        services.AddVSocket<ChatReceiver, ChatConnector>(options, "/chat", [|"Id"; "Name"; "ImageId"|], [|"eventId"|])
        services.AddSingleton(options) |> ignore
    
    [<Extension>]
    static member UseChatSockets(app: IApplicationBuilder) =
        app.UseWebSockets() |> ignore
        app.UseVSocket()
