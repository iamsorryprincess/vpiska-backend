module Vpiska.Application.Event.WebSocket.ConnectionsLogic

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
    }
    
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
    }
    
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
    }
    
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
    }
    
let handleConnection
    (createUserGroup: CreateUserGroup)
    (addUserInfo: AddUserInfo)
    (logError: LogError)
    (logErrors: LogErrors)
    (exist: CheckEvent)
    (commandHandler: HandleCommand)
    (connectionId: ConnectionId)
    (eventId: EventId)
    (userInfo: UserInfo) =
    task {
        match exist eventId with
        | true -> do! connectUser addUserInfo logError logErrors commandHandler connectionId eventId userInfo
        | false ->
            do! subscribe createUserGroup logError logErrors commandHandler eventId
            do! connectUser addUserInfo logError logErrors commandHandler connectionId eventId userInfo
    }
    
let handleDisconnection
    (removeUserGroup: RemoveUserGroup)
    (removeUserInfo: RemoveUserInfo)
    (getUsersCount: GetUsersCount)
    (logError: LogError)
    (logErrors: LogErrors)
    (commandHandler: HandleCommand)
    (connectionId: ConnectionId)
    (eventId: EventId)
    (userId: UserId) =
    task {
        match (getUsersCount eventId) - 1 with
        | 0 ->
            do! disconnectUser removeUserInfo logError logErrors commandHandler connectionId eventId userId
            do! unsubscribe removeUserGroup logError logErrors commandHandler eventId
        | _ -> do! disconnectUser removeUserInfo logError logErrors commandHandler connectionId eventId userId
    }
    
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
    }
