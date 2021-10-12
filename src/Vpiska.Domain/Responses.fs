namespace Vpiska.Domain.Responses

open Vpiska.Domain.Errors
  
type ErrorResponse = { ErrorCode: string }

[<AutoOpen>]
module private Response =
    let mapToErrorResponse (error: AppError) = { ErrorCode = Errors.mapAppError error }

type Response<'a> =
    { IsSuccess: bool
      Result: 'a
      Errors: ErrorResponse[] }
    static member success result = { IsSuccess = true; Result = result; Errors = [||] }
    static member error (errors: AppError[]) = { IsSuccess = false; Result = null; Errors = errors |> Array.map mapToErrorResponse }
 
type Response =
    { IsSuccess: bool
      Result: string
      Errors: ErrorResponse[] }
    static member success () = { IsSuccess = true; Result = null; Errors = [||] }
    static member error (errors: AppError[]) = { IsSuccess = false; Result = null; Errors = errors |> Array.map mapToErrorResponse }

type LoginResponse =
    { UserId: string
      AccessToken: string }
    
type DomainResponse =
    | Login of LoginResponse
    | Code
    | PasswordChanged
    | UserUpdated
