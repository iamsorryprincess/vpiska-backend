namespace Vpiska.Application.Event

open System
open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks
open Vpiska.Application
open Vpiska.Application.Event.CommandHandler
open Vpiska.Domain.Event

module internal Http =
    
    let private createMobileErrorResult (error: AppError) = { ErrorCode = Errors.mapAppError error }
    
    let private mapToMobileErrorResponse (errors: AppError[]) =
        ObjectResult({ IsSuccess = false; Result = null; Errors = errors |> Array.map createMobileErrorResult }, StatusCode = 200)
        
    let private mapToMobileResponse (response: Response) =
        match response with
        | EventCreated args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | _ -> raise(ArgumentException("Unknown response for event http mobile response"))
        
    let mapToMobileResult (result: Result<Response, AppError[]>) =
        match result with
        | Error errors -> mapToMobileErrorResponse errors
        | Ok response -> mapToMobileResponse response
        
type EventMobileHttpHandler(persistence: EventPersistence) =
    
    member _.Handle command =
        task {
            let! result = handle persistence command
            return Http.mapToMobileResult result
        }
