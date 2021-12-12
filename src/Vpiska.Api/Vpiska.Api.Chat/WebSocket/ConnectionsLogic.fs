module Vpiska.Api.Chat.WebSocket.ConnectionsLogic

open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Commands

type EventId = string
type ConnectionId = Guid
type UserId = string

type LogError = string -> unit
type LogErrors = AppError[] -> unit
type HandleCommand = ChatCommand -> Task<Result<DomainEvent, AppError[]>>

type CreateUserGroup = EventId -> bool
type RemoveUserGroup = EventId -> bool
type GetUserInfo = EventId -> ConnectionId -> UserInfo option
type AddUserInfo = EventId -> ConnectionId -> UserInfo -> bool
type RemoveUserInfo = EventId -> ConnectionId -> bool
type CheckEvent = EventId -> bool
type GetUsersCount = EventId -> int

type Deserialize = byte[] -> string

type Subscribe = EventId -> Task
type Unsubscribe = EventId -> Task
type Connect = ConnectionId -> EventId -> UserInfo -> Task
type Disconnect = ConnectionId -> EventId -> UserId -> Task

let private handle (commandHandler: HandleCommand) (logErrors: LogErrors) (command: ChatCommand) =
    task {
        match! commandHandler command with
        | Error errors -> logErrors errors
        | Ok _ -> return ()
    }

let subscribe
    (createUserGroup: CreateUserGroup)
    (logError: LogError)
    (logErrors: LogErrors)
    (commandHandler: HandleCommand)
    (eventId: EventId) =
    task {
        match createUserGroup eventId with
        | false -> logError $"can't create user group for eventId: {eventId}"
        | true ->
            let command = { EventId = eventId } |> ChatCommand.Subscribe
            do! handle commandHandler logErrors command
    } :> Task
    
let unsubscribe
    (removeUserGroup: RemoveUserGroup)
    (logError: LogError)
    (logErrors: LogErrors)
    (commandHandler: HandleCommand)
    (eventId: EventId) =
    task {
        match removeUserGroup eventId with
        | false -> logError $"can't remove user group for eventId: {eventId}"
        | true ->
            let command = { EventId = eventId } |> ChatCommand.Unsubscribe
            do! handle commandHandler logErrors command
    } :> Task
    
let connectUser
    (addUserInfo: AddUserInfo)
    (logError: LogError)
    (logErrors: LogErrors)
    (commandHandler: HandleCommand)
    (connectionId: ConnectionId)
    (eventId: EventId)
    (userInfo: UserInfo) =
    task {
        match addUserInfo eventId connectionId userInfo with
        | false -> logError $"can't add user connection. UserId: {userInfo.Id}. ConnectionId: {connectionId}"
        | true ->
            let command = { EventId = eventId; UserId = userInfo.Id
                            Name = userInfo.Name; ImageId = userInfo.ImageId } |> ChatCommand.LogUserInChat
            do! handle commandHandler logErrors command
    } :> Task
    
let disconnectUser
    (removeUserInfo: RemoveUserInfo)
    (logError: LogError)
    (logErrors: LogErrors)
    (commandHandler: HandleCommand)
    (connectionId: ConnectionId)
    (eventId: EventId)
    (userId: UserId) =
    task {
        match removeUserInfo eventId connectionId with
        | false -> logError $"can't remove user connection. UserId: {userId}. ConnectionId: {connectionId}"
        | true ->
            let command = { EventId = eventId; UserId = userId; } |> ChatCommand.LogoutUserFromChat
            do! handle commandHandler logErrors command
    } :> Task
    
let handleConnection
    (exist: CheckEvent)
    (connectUser: Connect)
    (subscribe: Subscribe)
    (connectionId: ConnectionId)
    (eventId: EventId)
    (userInfo: UserInfo) =
    task {
        match exist eventId with
        | true -> do! connectUser connectionId eventId userInfo
        | false ->
            do! subscribe eventId
            do! connectUser connectionId eventId userInfo
    } :> Task
    
let handleDisconnection
    (getUsersCount: GetUsersCount)
    (disconnectUser: Disconnect)
    (unsubscribe: Unsubscribe)
    (connectionId: ConnectionId)
    (eventId: EventId)
    (userId: UserId) =
    task {
        match (getUsersCount eventId) - 1 with
        | 0 ->
            do! disconnectUser connectionId eventId userId
            do! unsubscribe eventId
        | _ -> do! disconnectUser connectionId eventId userId
    } :> Task
    
let receiveMessage
    (getUserInfo: GetUserInfo)
    (deserialize: Deserialize)
    (commandHandler: HandleCommand)
    (logErrors: LogErrors)
    (connectionId: ConnectionId)
    (eventId: EventId)
    (userId: UserId)
    (data: byte[])=
    task {
        match getUserInfo eventId connectionId with
        | None -> return ()
        | Some userInfo ->
            let command = { EventId = eventId
                            UserId = userId
                            UserImage = userInfo.ImageId
                            Message = data |> deserialize } |> ChatCommand.SendChatMessage
            do! handle commandHandler logErrors command
    } :> Task
