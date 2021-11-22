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
        
    let private mapToMobileResponse (response: Response) =
        match response with
        | EventCreated args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | Response.EventClosed -> ObjectResult(HttpMobileResponse.createResult(), StatusCode = 200)
        | MediaAdded args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | MediaRemoved -> ObjectResult(HttpMobileResponse.createResult(), StatusCode = 200)
        | SubscriptionCreated -> raise(ArgumentException("Unknown response for event http mobile response"))
        | SubscriptionRemoved -> raise(ArgumentException("Unknown response for event http mobile response"))
        | UserLoggedInChat -> raise(ArgumentException("Unknown response for event http mobile response"))
        | UserLoggedOutFromChat -> raise(ArgumentException("Unknown response for event http mobile response"))
        | ChatMessageSent -> raise(ArgumentException("Unknown response for event http mobile response"))
        
    let private mapQueryResultToMobileResponse (queryResponse: QueryResponse) =
        match queryResponse with
        | Event args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | Events args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        
    let mapToMobileResult (result: Result<Response, AppError[]>) =
        match result with
        | Error errors -> mapToMobileErrorResponse errors
        | Ok response -> mapToMobileResponse response
        
    let mapQueryResultToMobileResult (result: Result<QueryResponse, AppError[]>) =
        match result with
        | Error errors -> mapToMobileErrorResponse errors
        | Ok response -> mapQueryResultToMobileResponse response
