module Vpiska.Domain.Event.Logic.QueriesLogic

open System
open FSharp.Control.Tasks
open Vpiska.Domain.Event

let private isNull o = Object.ReferenceEquals(o, null)

let getById
    (getEvent: GetEvent)
    (args: GetEventArgs) =
    task {
        match EventValidation.validateGetEventArgs args with
        | Error errors -> return errors |> Errors.fromValidation
        | Ok _ ->
            let! event = getEvent args.EventId
            match isNull event with
            | true -> return DomainError.EventNotFound |> Errors.fromDomain
            | false -> return event |> Response.Event |> Ok
    }
    
let getEvents
    (checkArea: CheckArea)
    (getEvents: GetEvents)
    (args: GetEventsArgs) =
    task {
        match EventValidation.validateGetEventsArgs args with
        | Error errors -> return errors |> Errors.fromValidation
        | Ok _ ->
            match checkArea args.Area with
            | false -> return DomainError.AreaNotFound |> Errors.fromDomain
            | true ->
                let! events = getEvents args.Area
                return events |> Response.Events |> Ok
    }
