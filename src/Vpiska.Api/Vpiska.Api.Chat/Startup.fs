namespace Vpiska.Api.Chat

open System
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Serilog
open Vpiska.Api.Chat.Connectors
open Vpiska.Api.Chat.Consumers
open Vpiska.Api.Chat.Receivers
open Vpiska.Infrastructure.Orleans
open Vpiska.Infrastructure.Websocket
open Vpiska.Infrastructure.Jwt

type Startup(configuration: IConfiguration) =

    member _.ConfigureServices(services: IServiceCollection) =
        // logger
        let logger = LoggerConfiguration()
                         .WriteTo.Console()
                         .WriteTo.File("logs/log-.txt", rollingInterval = RollingInterval.Day, outputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                         .CreateLogger()
        Log.Logger <- logger
        services.AddSingleton(Log.Logger) |> ignore
        
        // jwt
        services.AddJwt(configuration.GetSection("Jwt"))
        
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
        let forwardedHeaders = ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto
        app.UseForwardedHeaders(ForwardedHeadersOptions(ForwardedHeaders = forwardedHeaders)) |> ignore
