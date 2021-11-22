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
    
    member args.toCommand () =
        task {
            match Object.ReferenceEquals(args.Image, null) with
            | true -> return { Id = args.Id; Name = args.Name; Phone = args.Phone; ImageData = null; ContentType = null } |> Command.Update
            | false ->
                use stream = new MemoryStream()
                do! args.Image.CopyToAsync stream
                return { Id = args.Id; Name = args.Name; Phone = args.Phone
                         ImageData = stream.ToArray(); ContentType = args.Image.ContentType } |> Command.Update
        }
        
module Http =
    
    let private createMobileErrorResult (error: AppError) = { ErrorCode = Errors.mapAppError error }
    
    let private mapToMobileErrorResponse (errors: AppError[]) =
        ObjectResult({ IsSuccess = false; Result = null; Errors = errors |> Array.map createMobileErrorResult }, StatusCode = 200)
    
    let private mapToMobileResponse<'a> (response: Response) =
        match response with
        | UserLogged args -> ObjectResult(HttpMobileResponse.createValueResult args, StatusCode = 200)
        | Code -> ObjectResult(HttpMobileResponse.createResult (), StatusCode = 200)
        | PasswordChanged -> ObjectResult(HttpMobileResponse.createResult (), StatusCode = 200)
        | UserUpdated -> ObjectResult(HttpMobileResponse.createResult (), StatusCode = 200)
    
    let mapToMobileResult (result: Result<Response, AppError[]>) =
        match result with
        | Error errors -> mapToMobileErrorResponse errors
        | Ok response -> mapToMobileResponse response
