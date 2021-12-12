namespace Vpiska.Api.Chat.Receivers

open Serilog
open Vpiska.Api.Chat
open Vpiska.Api.Chat.Infrastructure
open Vpiska.Api.Chat.WebSocket
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Websocket

type ChatReceiver(logger: ILogger, persistence: Persistence) =
    
    let eventId = "eventId"
    let userId = "Id"
    
    let logErrors (errors: AppError[]) =
        for error in errors do
            error |> Errors.mapAppError |> logger.Error
            
    let handleCommand = CommandHandler.handle persistence
    
    let receive = ConnectionsLogic.receiveMessage Storage.getUserInfo WebSocketSerializer.deserializeToString handleCommand logErrors
    
    interface IWebSocketReceiver with
        member this.Receive(connectionId, data, identityParams, queryParams) =
            receive connectionId queryParams.[eventId] identityParams.[userId] data
