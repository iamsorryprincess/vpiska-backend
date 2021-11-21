namespace Vpiska.Application.Event

open System
open Microsoft.AspNetCore.Mvc
open Vpiska.Application
open Vpiska.Domain.Event

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
        
    let mapToMobileResult (result: Result<Response, AppError[]>) =
        match result with
        | Error errors -> mapToMobileErrorResponse errors
        | Ok response -> mapToMobileResponse response
