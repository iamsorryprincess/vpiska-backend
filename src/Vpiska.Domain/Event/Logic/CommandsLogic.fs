module Vpiska.Domain.Event.Logic.CommandsLogic

open FSharp.Control.Tasks
open Vpiska.Domain.Event

let createEvent
    (checkArea: CheckArea)
    (checkOwner: CheckOwner)
    (create: CreateEvent)
    (args: CreateEventArgs) =
    task {
        match EventValidation.validateCreateEventArgs args with
        | Error errors -> return errors |> Errors.fromValidation
        | Ok _ ->
            match checkArea args.Area with
            | false -> return DomainError.AreaNotFound |> Errors.fromDomain
            | true ->
                match! checkOwner args.Area args.OwnerId with
                | true -> return DomainError.OwnerAlreadyHasEvent |> Errors.fromDomain
                | false ->
                    let event = { Id = args.EventId; OwnerId = args.OwnerId; Name = args.Name; Coordinates = args.Coordinates
                                  Address = args.Address; MediaLinks = [||]; ChatData = [||]; Users = [||] }
                    match! create args.Area event with
                    | false -> return DomainError.AreaAlreadyHasEvent |> Errors.fromDomain
                    | true -> return DomainEvent.EventCreated event |> Ok
    }
    
let closeEvent
    (checkEvent: CheckEvent)
    (checkOwnership: CheckOwnership)
    (publish: PublishEvent)
    (closeEvent: CloseEvent)
    (args: CloseEventArgs) =
    task {
        match EventValidation.validateCloseEventArgs args with
        | Error errors -> return errors |> Errors.fromValidation
        | Ok _ ->
            match! checkEvent args.EventId with
            | false -> return DomainError.EventNotFound |> Errors.fromDomain
            | true ->
                match! checkOwnership args.EventId args.OwnerId with
                | false -> return DomainError.UserNotOwner |> Errors.fromDomain
                | true ->
                    match! closeEvent args.EventId with
                    | false -> return DomainError.EventNotFound |> Errors.fromDomain
                    | true ->
                        let domainEvent = DomainEvent.EventClosed
                        do! publish args.EventId domainEvent
                        return Ok domainEvent
    }
    
let subscribe (createSubscription: CreateSubscription) (args: SubscribeArgs) =
    task {
        match! createSubscription args.EventId with
        | false -> return DomainError.SubscriptionAlreadyExist |> Errors.fromDomain
        | true -> return DomainEvent.SubscriptionCreated |> Ok
    }
    
let unsubscribe (removeSubscription: RemoveSubscription) (args: SubscribeArgs) =
    task {
        match! removeSubscription args.EventId with
        | false -> return DomainError.SubscriptionNotFound |> Errors.fromDomain
        | true -> return DomainEvent.SubscriptionRemoved |> Ok
    }
    
let connectUserToChat
    (checkEvent: CheckEvent)
    (getUsers: GetUsers)
    (addUser: AddUser)
    (publish: PublishEvent)
    (args: LoginUserArgs) =
    task {
        match! checkEvent args.EventId with
        | false -> return DomainError.EventNotFound |> Errors.fromDomain
        | true ->
            let userInfo = { Id = args.UserId; Name = args.Name; ImageId = args.ImageId }
            let! users = getUsers args.EventId
            match! addUser args.EventId userInfo with
            | false -> return DomainError.UserAlreadyExists |> Errors.fromDomain
            | true ->
                let domainEvent = DomainEvent.UserLoggedIn { User = userInfo; OtherUsers = users }
                do! publish args.EventId domainEvent
                return Ok domainEvent
    }
    
let disconnectUserFromChat
    (checkEvent: CheckEvent)
    (removeUser: RemoveUser)
    (publish: PublishEvent)
    (args: LogoutUserArgs) =
    task {
        match! checkEvent args.EventId with
        | false -> return DomainError.EventNotFound |> Errors.fromDomain
        | true ->
            match! removeUser args.EventId args.UserId with
            | false -> return DomainError.UserNotFound |> Errors.fromDomain
            | true ->
                let domainEvent = DomainEvent.UserLoggedOut { UserId = args.UserId }
                do! publish args.EventId domainEvent
                return Ok domainEvent
    }
    
let sendChatMessage
    (checkEvent: CheckEvent)
    (addMessage: AddMessage)
    (publish: PublishEvent)
    (args: ChatMessageArgs) =
    task {
        match! checkEvent args.EventId with
        | false -> return DomainError.EventNotFound |> Errors.fromDomain
        | true ->
            let chatData = { UserId = args.UserId; Message = args.Message; UserImage = args.UserImage }
            match! addMessage args.EventId chatData with
            | false -> return DomainError.UserNotFound |> Errors.fromDomain
            | true ->
                let domainEvent = chatData |> DomainEvent.ChatMessageSent
                do! publish args.EventId domainEvent
                return Ok domainEvent
    }
    
let addMedia
    (checkEvent: CheckEvent)
    (checkOwnership: CheckOwnership)
    (addMedia: AddMedia)
    (uploadFile: UploadFile)
    (args: AddMediaArgs) =
    task {
        match EventValidation.validateAddMediaArgs args with
        | Error errors -> return errors |> Errors.fromValidation
        | Ok _ ->
            match! checkEvent args.EventId with
            | false -> return DomainError.EventNotFound |> Errors.fromDomain
            | true ->
                match! checkOwnership args.EventId args.OwnerId with
                | false -> return DomainError.UserNotOwner |> Errors.fromDomain
                | true ->
                    match! addMedia args.EventId args.ImageId with
                    | false -> return DomainError.MediaAlreadyAdded |> Errors.fromDomain
                    | true ->
                        let! result = uploadFile args.ImageId args.MediaData args.ContentType
                        return { ImageId = result } |> DomainEvent.MediaAdded |> Ok
    }
    
let removeMedia
    (checkEvent: CheckEvent)
    (checkOwnership: CheckOwnership)
    (removeMedia: RemoveMedia)
    (deleteFile: DeleteFile)
    (args: RemoveMediaArgs) =
    task {
        match EventValidation.validateRemoveMediaArgs args with
        | Error errors -> return errors |> Errors.fromValidation
        | Ok _ ->
            match! checkEvent args.EventId with
            | false -> return DomainError.EventNotFound |> Errors.fromDomain
            | true ->
                match! checkOwnership args.EventId args.OwnerId with
                | false -> return DomainError.UserNotOwner |> Errors.fromDomain
                | true ->
                    match! removeMedia args.EventId args.MediaLink with
                    | false -> return DomainError.MediaNotFound |> Errors.fromDomain
                    | true ->
                        let! _ = deleteFile args.MediaLink
                        return { ImageId = args.MediaLink } |> DomainEvent.MediaRemoved |> Ok
    }
