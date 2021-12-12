namespace Vpiska.Api.Control.Controllers

open System
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks
open Vpiska.Api.Control.CommandHandlers
open Vpiska.Api.Control.Http
open Vpiska.Api.Control.QueryHandlers
open Vpiska.Domain.Event

[<Route "api/events">]
type EventsController() as this =
    inherit ControllerBase()
    
    let getOwnerId () =
        let claim = this.HttpContext.User.Claims |> Seq.find (fun item -> item.Type = "Id")
        let mutable guid = Guid.Empty
        if Guid.TryParseExact(claim.Value, "N", &guid) then
            claim.Value
        else
            raise(InvalidOperationException("Can't resolve userId token from request"))
    
    [<HttpPost "all">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse<ShortEventResponse[]>>, 200)>]
    member _.GetAll([<FromServices>] persistence, [<FromBody>] args) =
        task {
            let query = args |> Query.GetEvents
            let! result = EventQueryHandler.handle persistence query
            return EventHttpResponse.fromResponseResult result
        }
        
    [<HttpPost "single">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse<Event>>, 200)>]
    member _.GetById([<FromServices>] persistence, [<FromBody>] args) =
        task {
            let query = args |> Query.GetEvent
            let! result = EventQueryHandler.handle persistence query
            return EventHttpResponse.fromResponseResult result
        }
    
    [<Authorize>]
    [<HttpPost "create">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse<Event>>, 200)>]
    member _.Create([<FromServices>] persistence, [<FromBody>] args: CreateEventArgs) =
        task {
            let eventId = Guid.NewGuid().ToString("N")
            let ownerId = getOwnerId ()
            let command = args.toCommand eventId ownerId
            let! result = EventCommandHandler.handle persistence command
            return EventHttpResponse.fromEventResult result
        }
        
    [<Authorize>]
    [<HttpPost "close">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse>, 200)>]
    member _.Close([<FromServices>] persistence, [<FromBody>] args: CloseEventArgs) =
        task {
            let ownerId = getOwnerId ()
            let command = args.toCommand ownerId
            let! result = EventCommandHandler.handle persistence command
            return EventHttpResponse.fromEventResult result
        }
        
    [<Authorize>]
    [<HttpPost "media/add">]
    [<Produces "application/json">]
    [<Consumes "multipart/form-data">]
    [<ProducesResponseType(typeof<ApiResponse<MediaArgs>>, 200)>]
    member _.AddMedia([<FromServices>] persistence, [<FromForm>] args: AddMediaArgs) =
        task {
            let imageId = Guid.NewGuid().ToString("N")
            let ownerId = getOwnerId ()
            let! command = args.toCommand ownerId imageId
            let! result = EventCommandHandler.handle persistence command
            return EventHttpResponse.fromEventResult result
        }
        
    [<Authorize>]
    [<HttpPost "media/remove">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse>, 200)>]
    member _.RemoveMedia([<FromServices>] persistence, [<FromBody>] args: RemoveMediaArgs) =
        task {
            let ownerId = getOwnerId ()
            let command = args.toCommand ownerId
            let! result = EventCommandHandler.handle persistence command
            return EventHttpResponse.fromEventResult result
        }
