namespace Vpiska.Api.Chat

open System
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens
open Serilog
open Vpiska.Api.Chat.Connectors
open Vpiska.Api.Chat.Consumers
open Vpiska.Api.Chat.Infrastructure
open Vpiska.Api.Chat.Receivers
open Vpiska.Infrastructure.Orleans
open Vpiska.Infrastructure.Websocket

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
        
    static let handle (context: MessageReceivedContext) (tokenQueryParam: string) (webSocketUrl: string) =
        match context.Request.Query.Count with
        | 0 -> Task.CompletedTask
        | _ ->
            if context.Request.Query.ContainsKey tokenQueryParam |> not then
                Task.CompletedTask
            else
                let accessToken = context.Request.Query.[tokenQueryParam].Item 0
                let path = context.HttpContext.Request.Path
                if String.IsNullOrWhiteSpace(accessToken) |> not && path.StartsWithSegments(PathString(webSocketUrl)) then
                    context.Token <- accessToken
                Task.CompletedTask

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
                                         options.TokenValidationParameters <- createTokenParams()
                                         options.Events <- JwtBearerEvents(OnMessageReceived =
                                             Func<MessageReceivedContext, Task>(fun context -> handle context "access_token" "/chat"))) |> ignore
        services.AddAuthorization() |> ignore
        
        // orleans
        services.AddClusterClient(configuration.GetSection("OrleansCluster"))
        services.AddStreamProducer()
        services.AddStreamConsumer<ChatConsumer>()
        
        // persistence
        services.AddTransient<Persistence>() |> ignore
        
        // chat
        let options = WebSocketsOptions()
        let idGen = Dictionary<string, Func<string>>()
        idGen.Add("Id", Func<string>(fun () -> Guid.NewGuid().ToString("N")))
        services.AddVSocket<ChatReceiver, ChatConnector>(options, "/chat", [|"Id"; "Name"; "ImageId"|], [|"eventId"|], idGen)
        services.AddSingleton(options) |> ignore

    member _.Configure(app: IApplicationBuilder) =
        app.UseRouting() |> ignore
        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore
        app.UseWebSockets() |> ignore
        app.UseVSocket()
