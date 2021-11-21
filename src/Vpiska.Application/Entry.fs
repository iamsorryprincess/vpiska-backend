namespace Vpiska.Application

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Vpiska.Application.Event.WebSocket
open Vpiska.Infrastructure.Jwt
open Vpiska.Infrastructure.Websocket
open Vpiska.Infrastructure.Orleans
open Vpiska.Application.Event.CommandHandler
open Vpiska.Application.User.CommandHandler

[<Extension>]
type Entry() =
    
    [<Extension>]
    static member AddUserPersistence(services: IServiceCollection) =
        services.AddSingleton<UserPersistence>() |> ignore
    
    [<Extension>]
    static member AddEventPersistence(services: IServiceCollection, areas: string[]) =
        services.AddStreamProducer()
        let areaSettings = { Areas = areas }
        services.AddSingleton(areaSettings) |> ignore
        services.AddSingleton<EventPersistence>() |> ignore
    
    [<Extension>]
    static member AddEventChat(services: IServiceCollection) =
        services.AddStreamConsumer<ChatConsumer>()
        services.AddJwtForWebsocket("access_token", "/chat")
        let options = WebSocketsOptions()
        services.AddVSocket<ChatReceiver, ChatConnector>(options, "/chat", "eventId")
        services.AddSingleton(options) |> ignore
    
    [<Extension>]
    static member UseChatSockets(app: IApplicationBuilder) =
        app.UseWebSockets() |> ignore
        app.UseVSocket()
