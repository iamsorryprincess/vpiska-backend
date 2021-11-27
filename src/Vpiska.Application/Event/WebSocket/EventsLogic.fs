module Vpiska.Application.Event.WebSocket.EventsLogic

open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open Vpiska.Domain.Event

type EventId = string
type ConnectionId = Guid
type UserId = string

type LogError = string -> unit
type Serialize<'a> = 'a -> byte[]
type SendMessage = ConnectionId -> byte[] -> Task
type Close = ConnectionId -> Task

type GetUserConnection = UserId -> ConnectionId option
type GetUsersConnection = EventId -> ConnectionId[] option
type GetUsersConnectionsExceptOne = EventId -> UserId -> ConnectionId[] option

type RemoveUserGroup = EventId -> bool
type RemoveUserInfo = EventId -> ConnectionId -> bool

let private sendToConnections (send: SendMessage) (connections: ConnectionId[]) (data: byte[]) =
    connections |> Array.map (fun c -> send c data) |> Task.WhenAll
        
let private closeConnections (close: Close) (connections: ConnectionId[]) =
    connections |> Array.map close |> Task.WhenAll

let handleUserLoggedIn
    (getUserConnection: GetUserConnection)
    (getConnectionsExceptOne: GetUsersConnectionsExceptOne)
    (serialize: Serialize<WebSocketResponse>)
    (send: SendMessage)
    (eventId: string)
    (args: UserLoggedInArgs) =
    task {
        match getUserConnection args.User.Id with
        | None -> return ()
        | Some userConnection ->
            match getConnectionsExceptOne eventId args.User.Id with
            | None -> return ()
            | Some otherConnections ->
                let userMessage = { Type = MessageType.UsersInfo; Data = args.OtherUsers } |> serialize
                let otherUsersMessage = { Type = MessageType.UserLoggedIn; Data = args.User } |> serialize
                do! sendToConnections send otherConnections otherUsersMessage
                do! send userConnection userMessage
    }
    
let handleUserLoggedOut
    (getConnections: GetUsersConnection)
    (serialize: Serialize<WebSocketResponse>)
    (send: SendMessage)
    (eventId: string)
    (args: UserLoggedOutArgs) =
    task {
        match getConnections eventId with
        | None -> return ()
        | Some connections ->
            let message = { Type = MessageType.UserLoggedOut; Data = args } |> serialize
            do! sendToConnections send connections message
    }
    
let handleChatMessage
    (getConnections: GetUsersConnection)
    (serialize: Serialize<WebSocketResponse>)
    (send: SendMessage)
    (eventId: string)
    (chatData: ChatData) =
    task {
        match getConnections eventId with
        | None -> return ()
        | Some connections ->
            let message = { Type = MessageType.ChatMessage; Data = chatData } |> serialize
            do! sendToConnections send connections message
    }
    
let handleEventClosed
    (getConnections: GetUsersConnection)
    (removeUserInfo: RemoveUserInfo)
    (removeUserGroup: RemoveUserGroup)
    (serialize: Serialize<WebSocketResponse>)
    (send: SendMessage)
    (close: Close)
    (log: LogError)
    (eventId: string) =
    task {
        match getConnections eventId with
        | None -> return ()
        | Some connections ->
            match connections |> Array.forall (removeUserInfo eventId) with
            | false -> log "can't remove connections"
            | true ->
                match removeUserGroup eventId with
                | false -> log "can't remove user group"
                | true ->
                    let message = { Type = MessageType.EventClosed; Data = null } |> serialize
                    do! sendToConnections send connections message
                    do! closeConnections close connections
    }
