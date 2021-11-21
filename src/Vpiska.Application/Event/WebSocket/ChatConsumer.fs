namespace Vpiska.Application.Event.WebSocket

open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Interfaces
open Vpiska.Infrastructure.Websocket

type ChatConsumer(storage: UserConnectionsStorage, sender: IWebSocketSender<ChatConnector>) =
    
    let sendToConnections (connections: Guid[]) (data: byte[]) = connections |> Array.map (fun c -> sender.SendMessage(c, data)) |> Task.WhenAll
    
    let handleUserLoggedIn (eventId: string) (args: UserLoggedInArgs) =
        task {
            let mutable userConnection = Guid.Empty
            match storage.TryGetUserConnectionId (args.User.Id, &userConnection) with
            | false -> return ()
            | true ->
                let mutable otherConnections = [||]
                match storage.TryGetUsersConnectionsWithoutOne (eventId, args.User.Id, &otherConnections) with
                | false -> return ()
                | true ->
                    let userMessage = { Type = MessageType.UsersInfo; Data = args.OtherUsers } |> WebSocketSerializer.serialize
                    let otherUsersMessage = { Type = MessageType.UserLoggedIn; Data = args.User } |> WebSocketSerializer.serialize
                    do! sendToConnections otherConnections otherUsersMessage
                    do! sender.SendMessage (userConnection, userMessage)
        } :> Task
        
    let handleUserLoggedOut (eventId: string) (args: UserLoggedOutArgs) =
        task {
            let mutable connections = [||]
            match storage.TryGetUsersConnections (eventId, &connections) with
            | false -> return ()
            | true ->
                let message = { Type = MessageType.UserLoggedOut; Data = args } |> WebSocketSerializer.serialize
                do! sendToConnections connections message
        } :> Task
        
    let handleChatMessage (eventId: string) (chatData: ChatData) =
        task {
            let mutable connections = [||]
            match storage.TryGetUsersConnections (eventId, &connections) with
            | false -> return ()
            | true ->
                let message = { Type = MessageType.ChatMessage; Data = chatData } |> WebSocketSerializer.serialize
                do! sendToConnections connections message
        } :> Task
    
    interface IStreamConsumer with
        member _.Consume(eventId, domainEvent) =
            match domainEvent with
            | UserLoggedIn args -> handleUserLoggedIn eventId args
            | UserLoggedOut args -> handleUserLoggedOut eventId args
            | ChatMessage args -> handleChatMessage eventId args
