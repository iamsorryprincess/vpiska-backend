namespace Vpiska.Api

open System.IO
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Vpiska.Domain.Commands
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
    
[<CLIMutable>]
type UpdateUserRequest =
    { Id: string
      Name: string
      Phone: string
      Image: IFormFile }

module Http =
    
    let private mapToErrorResponse (error: AppError) = { ErrorCode = Errors.mapAppError error }
    
    let private createValueResult<'a> (result: 'a): Response<'a> = { IsSuccess = true; Result = result; Errors = [||] }
    
    let private createResult () = { IsSuccess = true; Result = null; Errors = [||] }
    
    let createErrorResult (errors: AppError[]) = { IsSuccess = false; Result = null; Errors = errors |> Array.map mapToErrorResponse }
    
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
        
    let mapUpdateRequest (request: UpdateUserRequest) =
        task {
            if request.Image = null then
                return { Id = request.Id; Name = request.Name; Phone = request.Phone
                         ImageData = ValueNone; ContentType = null }
            else
                use stream = new MemoryStream()
                do! request.Image.CopyToAsync(stream)
                return { Id = request.Id; Name = request.Name; Phone = request.Phone
                         ImageData = ValueSome (stream.ToArray()); ContentType = request.Image.ContentType }
        }
