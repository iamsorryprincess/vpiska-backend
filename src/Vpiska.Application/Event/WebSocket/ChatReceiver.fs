namespace Vpiska.Application.Event.WebSocket

open System.Threading.Tasks
open FSharp.Control.Tasks
open Serilog
open Vpiska.Application.Event
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Websocket

type ChatReceiver(logger: ILogger, storage: UserConnectionsStorage, commandHandler: CommandHandler) =
    
    let queryParamName = "eventId"
    
    let mapErrors errors = errors |> Array.map Errors.mapAppError |> Array.fold (fun acc error -> $"{error}. {acc}") ("")
    
    interface IWebSocketReceiver with
        member this.Receive(connectionId, data, queryParams) =
            task {
                let eventId = queryParams.[queryParamName]
                let mutable context = null
                match storage.TryGetUserInfo (eventId, connectionId, &context) with
                | false -> logger.Warning("can't get userInfo while receive message. EventId: {eventId}, ConnectionId: {connectionId}", eventId, connectionId)
                | true ->
                    let command = { EventId = eventId; UserId = context.UserId; Message = data |> WebSocketSerializer.deserializeToString }
                                  |> Command.SendChatMessage
                    match! commandHandler.Handle command with
                    | Error errors ->
                        let message = mapErrors errors
                        logger.Warning("can't send message. Message: {message}", message)
                    | Ok _ -> return ()
            } :> Task
