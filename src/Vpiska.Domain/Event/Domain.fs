module Vpiska.Domain.Event.Domain

open System
open System.Threading.Tasks
open FSharp.Control.Tasks

type Area = string
type CheckArea = Area -> bool
type CheckOwner = Area -> UserId -> Task<bool>
type CreateEvent = Area -> Event -> Task<bool>

type CreateSubscription = EventId -> Task<bool>
type RemoveSubscription = EventId -> Task<bool>
type PublishEvent = EventId -> DomainEvent -> Task

type CheckEvent = EventId -> Task<bool>
type AddUser = EventId -> UserInfo -> Task<bool>
type GetUsers = EventId -> Task<UserInfo[]>
type RemoveUser = EventId -> UserId -> Task<bool>
type AddMessage = EventId -> ChatData -> Task<bool>

let private generateId () = Guid.NewGuid().ToString("N")

let createEvent
    (checkArea: CheckArea)
    (checkOwner: CheckOwner)
    (create: CreateEvent)
    (args: CreateEventArgs) =
    task {
        match EventValidation.validateCreateEventArgs args with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            match checkArea args.Area with
            | false -> return [| DomainError.AreaNotFound |> AppError.create |] |> Error
            | true ->
                match! checkOwner args.Area args.OwnerId with
                | true -> return [| DomainError.OwnerAlreadyHasEvent |> AppError.create |] |> Error
                | false ->
                    let event = { Id = generateId(); OwnerId = args.OwnerId; Name = args.Name; Coordinates = args.Coordinates
                                  Address = args.Address; MediaLinks = [||]; ChatData = [||]; Users = [||] }
                    match! create args.Area event with
                    | false -> return [| DomainError.AreaAlreadyHasEvent |> AppError.create |] |> Error
                    | true -> return Response.EventCreated event |> Ok
    }
    
let subscribe (createSubscription: CreateSubscription) (args: SubscribeArgs) =
    task {
        match! createSubscription args.EventId with
        | false -> return [| DomainError.SubscriptionAlreadyExist |> AppError.create |] |> Error
        | true -> return Response.SubscriptionCreated |> Ok
    }
    
let unsubscribe (removeSubscription: RemoveSubscription) (args: SubscribeArgs) =
    task {
        match! removeSubscription args.EventId with
        | false -> return [| DomainError.SubscriptionNotFound |> AppError.create |] |> Error
        | true -> return Response.SubscriptionRemoved |> Ok
    }
    
let connectUserToChat
    (checkEvent: CheckEvent)
    (getUsers: GetUsers)
    (addUser: AddUser)
    (publish: PublishEvent)
    (args: LoginUserArgs) =
    task {
        match! checkEvent args.EventId with
        | false -> return [| DomainError.EventNotFound |> AppError.create |] |> Error
        | true ->
            let userInfo = { Id = args.UserId; Name = args.Name; ImageId = args.ImageId }
            let! users = getUsers args.EventId
            match! addUser args.EventId userInfo with
            | false -> return [| DomainError.UserAlreadyExists |> AppError.create |] |> Error
            | true ->
                let domainEvent = DomainEvent.UserLoggedIn { User = userInfo; OtherUsers = users }
                do! publish args.EventId domainEvent
                return Ok Response.UserLoggedInChat
    }
    
let disconnectUserFromChat
    (checkEvent: CheckEvent)
    (removeUser: RemoveUser)
    (publish: PublishEvent)
    (args: LogoutUserArgs) =
    task {
        match! checkEvent args.EventId with
        | false -> return [| DomainError.EventNotFound |> AppError.create |] |> Error
        | true ->
            match! removeUser args.EventId args.UserId with
            | false -> return [| DomainError.UserNotFound |> AppError.create |] |> Error
            | true ->
                let domainEvent = DomainEvent.UserLoggedOut { UserId = args.UserId }
                do! publish args.EventId domainEvent
                return Ok Response.UserLoggedOutFromChat
    }
    
let sendChatMessage
    (checkEvent: CheckEvent)
    (addMessage: AddMessage)
    (publish: PublishEvent)
    (args: ChatMessageArgs) =
    task {
        match! checkEvent args.EventId with
        | false -> return [| DomainError.EventNotFound |> AppError.create |] |> Error
        | true ->
            let chatData = { UserId = args.UserId; Message = args.Message }
            match! addMessage args.EventId chatData with
            | false -> return [| DomainError.UserNotFound |> AppError.create |] |> Error
            | true ->
                do! publish args.EventId (chatData |> DomainEvent.ChatMessage)
                return Ok Response.ChatMessageSent
    }
