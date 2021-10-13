namespace Vpiska.Api

open Vpiska.Domain.Errors
open Vpiska.Domain.Responses

type ErrorResponse = { ErrorCode: string }

type Response<'a> =
    { IsSuccess: bool
      Result: 'a
      Errors: ErrorResponse[] }
 
type Response =
    { IsSuccess: bool
      Result: string
      Errors: ErrorResponse[] }

module Http =
    
    let private mapToErrorResponse (error: AppError) = { ErrorCode = Errors.mapAppError error }
    
    let private createValueResult<'a> (result: 'a): Response<'a> = { IsSuccess = true; Result = result; Errors = [||] }
    
    let private createResult () = { IsSuccess = true; Result = null; Errors = [||] }
    
    let private createErrorResult (errors: AppError[]) = { IsSuccess = false; Result = null; Errors = errors |> Array.map mapToErrorResponse }
    
    let private mapResult<'a> result =
        match result with
        | Login args -> createValueResult args :> obj
        | Code -> createResult () :> obj
        | PasswordChanged -> createResult () :> obj
        | UserUpdated -> createResult () :> obj
    
    let mapResponse (response: Result<DomainResponse, AppError[]>) =
        match response with
        | Error errors -> createErrorResult errors :> obj
        | Ok result -> mapResult result
