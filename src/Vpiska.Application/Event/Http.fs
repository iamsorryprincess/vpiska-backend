namespace Vpiska.Application.Event

open System
open System.IO
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Vpiska.Application
open Vpiska.Domain.Event

[<CLIMutable>]
type AddMediaArgs =
    { EventId: string
      Media: IFormFile }
    member this.toCommand (ownerId: string) =
        task {
            match Object.ReferenceEquals(this.Media, null) with
            | true -> return { EventId = this.EventId; OwnerId = ownerId; MediaData = null; ContentType = null } |> Command.AddMedia
            | false ->
                use stream = new MemoryStream()
                do! this.Media.CopyToAsync stream
                return { EventId = this.EventId; OwnerId = ownerId; MediaData = stream.ToArray(); ContentType = this.Media.ContentType } |> Command.AddMedia
        }

[<CLIMutable>]
type RemoveMediaArgs =
    { EventId: string
      MediaLink: string }
    member this.toCommand (ownerId: string) =
        { EventId = this.EventId; OwnerId = ownerId; MediaLink = this.MediaLink } |> Command.RemoveMedia

[<CLIMutable>]
type CreateEventArgs =
    { Name: string
      Coordinates: string
      Address: string
      Area: string }
    member this.toCommand (ownerId: string) =
        { OwnerId = ownerId; Name = this.Name; Coordinates = this.Coordinates
          Address = this.Address; Area = this.Area } |> Command.CreateEvent
    
[<CLIMutable>]    
type CloseEventArgs =
    { EventId: string }
    member this.toCommand (ownerId: string) = { EventId = this.EventId; OwnerId = ownerId } |> Command.CloseEvent

module Http =
    
    let private createMobileErrorResult (error: AppError) = { ErrorCode = Errors.mapAppError error }
    
    let private mapToMobileErrorResponse (errors: AppError[]) =
        ObjectResult({ IsSuccess = false; Result = null; Errors = errors |> Array.map createMobileErrorResult }, StatusCode = 200)
        
    let private fromEventToMobile (domainEvent: DomainEvent) =
        match domainEvent with
        | EventCreated args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | EventClosed -> ObjectResult(HttpMobileResponse.createResult(), StatusCode = 200)
        | MediaAdded args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | MediaRemoved _ -> ObjectResult(HttpMobileResponse.createResult(), StatusCode = 200)
        | SubscriptionCreated -> raise(ArgumentException("Unknown event for http mobile response"))
        | SubscriptionRemoved -> raise(ArgumentException("Unknown event for http mobile response"))
        | UserLoggedIn _ -> raise(ArgumentException("Unknown event for http mobile response"))
        | UserLoggedOut _ -> raise(ArgumentException("Unknown event for http mobile response"))
        | ChatMessageSent _ -> raise(ArgumentException("Unknown event for http mobile response"))
        
    let private fromResponseToMobile (response: Response) =
        match response with
        | Event args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | Events args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        
    let mapToMobileEventResult (result: Result<DomainEvent, AppError[]>) =
        match result with
        | Error errors -> mapToMobileErrorResponse errors
        | Ok domainEvent -> fromEventToMobile domainEvent
        
    let mapToMobileResponseResult (result: Result<Response, AppError[]>) =
        match result with
        | Error errors -> mapToMobileErrorResponse errors
        | Ok response -> fromResponseToMobile response
