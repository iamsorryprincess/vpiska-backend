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
