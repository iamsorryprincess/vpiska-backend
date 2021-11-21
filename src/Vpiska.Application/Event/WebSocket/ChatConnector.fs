namespace Vpiska.Application.Event.WebSocket

open System
open System.Threading.Tasks
open Serilog
open FSharp.Control.Tasks
open Vpiska.Application.Event
open Vpiska.Application.Event.CommandHandler
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Websocket

type ChatConnector(logger: ILogger, storage: UserConnectionsStorage, persistence: EventPersistence) =
    
    let queryParamName = "eventId"
    
    let handle = CommandHandler.handle persistence
    
    let mapErrors errors = errors |> Array.map Errors.mapAppError |> Array.fold (fun acc error -> $"{error}. {acc}") ""
    
    let connectUser (eventId: string) (connectionId: Guid) (userContext: WebSocketUserContext) =
        task {
            match storage.TryAddUserContext (eventId, connectionId, userContext) with
            | false -> logger.Warning("can't add user context. EventId: {eventId}. ConnectionId: {connectionId}", eventId, connectionId)
            | true ->
                let args = { EventId = eventId; UserId = userContext.UserId; Name = userContext.Username; ImageId = userContext.UserImageId }
                match! args |> Command.LogUserInChat |> handle with
                | Error errors ->
                    let message = mapErrors errors
                    logger.Warning("can't connect user to event. Message: {message}", message)
                | Ok _ -> return ()
        } :> Task
        
    let disconnectUser (eventId: string) (userId: string) =
        task {
            let command = { EventId = eventId; UserId = userId } |> Command.LogoutUserFromChat
            match! handle command with
            | Error errors ->
                let message = mapErrors errors
                logger.Warning("can't disconnect user from event. Message: {message}", message)
            | Ok _ -> return ()
        } :> Task
    
    interface IWebSocketConnector with
        member _.OnConnect(connectionId, userContext, queryParams) =
            task {
                let eventId = queryParams.[queryParamName]
                match storage.Exist eventId with
                | true -> do! connectUser eventId connectionId userContext
                | false ->
                    match storage.TryCreateUserGroup eventId with
                    | false -> logger.Warning("can't create user group. EventId: {eventId}. ConnectionId: {connectionId}", eventId, connectionId)
                    | true ->
                        let command = { EventId = eventId } |> Command.Subscribe
                        match! handle command with
                        | Error errors ->
                            let message = mapErrors errors
                            logger.Warning("can't subscribe for event. Message: {message}", message)
                        | Ok _ -> do! connectUser eventId connectionId userContext
            } :> Task
            
        
        member _.OnDisconnect(connectionId, queryParams) =
            task {
                let eventId = queryParams.[queryParamName]
                let mutable userContext = null
                match storage.TryRemoveUserContext (eventId, connectionId, &userContext) with
                | false -> logger.Warning("can't remove user context. EventId: {eventId}. ConnectionId: {connectionId}", eventId, connectionId)
                | true ->
                    do! disconnectUser eventId userContext.UserId
                    match storage.GetUsersCount eventId with
                    | 0 ->
                        match storage.TryRemoveUserGroup eventId with
                        | false -> logger.Warning("can't remove user group. EventId: {eventId}. ConnectionId: {connectionId}", eventId, connectionId)
                        | true ->
                            let command = { EventId = eventId } |> Command.Unsubscribe
                            match! handle command with
                            | Error errors ->
                                let message = mapErrors errors
                                logger.Warning("can't unsubscribe from event. Message: {message}", message)
                            | Ok _ -> return ()
                    | _ -> return ()
            } :> Task
