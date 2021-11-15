namespace Vpiska.Application.User

open System
open System.IO
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Mvc
open Vpiska.Application
open Vpiska.Domain.User

[<CLIMutable>]
type UpdateUserArgs =
    { Id: string
      Name: string
      Phone: string
      Image: IFormFile }
    
    member args.toCommandArgs () =
        task {
            match Object.ReferenceEquals(args.Image, null) with
            | true -> return { Id = args.Id; Name = args.Name; Phone = args.Phone; ImageData = null; ContentType = null }
            | false ->
                use stream = new MemoryStream()
                do! args.Image.CopyToAsync stream
                return { Id = args.Id; Name = args.Name; Phone = args.Phone
                         ImageData = stream.ToArray(); ContentType = args.Image.ContentType }
        }
        
module internal Http =
    
    let private mapToMobileErrorResponse (error: AppError) = { ErrorCode = Errors.mapAppError error }
    
    let private createMobileErrorResult (errors: AppError[]) =
        ObjectResult({ IsSuccess = false; Result = null; Errors = errors |> Array.map mapToMobileErrorResponse }, StatusCode = 200)
    
    let private mapToMobileResponse<'a> (response: Response) =
        match response with
        | UserLogged args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | Code -> ObjectResult(HttpMobileResponse.createResult (), StatusCode = 200)
        | PasswordChanged -> ObjectResult(HttpMobileResponse.createResult (), StatusCode = 200)
        | UserUpdated -> ObjectResult(HttpMobileResponse.createResult (), StatusCode = 200)
    
    let mapToMobileResult (result: Result<Response, AppError[]>) =
        match result with
        | Error errors -> createMobileErrorResult errors
        | Ok response -> mapToMobileResponse response
