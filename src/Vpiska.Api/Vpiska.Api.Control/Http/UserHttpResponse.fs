namespace Vpiska.Api.Control.Http

open System
open System.IO
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Mvc
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

module UserHttpResponse =
    
    let private createErrorResult (error: AppError) = { ErrorCode = Errors.mapAppError error }
    
    let private mapErrors (errors: AppError[]) =
        ObjectResult({ IsSuccess = false; Result = null; Errors = errors |> Array.map createErrorResult }, StatusCode = 200)
        
    let private mapEvent<'a> (domainEvent: DomainEvent) =
        match domainEvent with
        | UserLogged args -> ObjectResult(HttpResponse.createValueResult args, StatusCode = 200)
        | CodePushed -> ObjectResult(HttpResponse.createResult (), StatusCode = 200)
        | PasswordChanged -> ObjectResult(HttpResponse.createResult (), StatusCode = 200)
        | UserUpdated -> ObjectResult(HttpResponse.createResult (), StatusCode = 200)
        
    let fromEventResult (result: Result<DomainEvent, AppError[]>) =
        match result with
        | Error errors -> mapErrors errors
        | Ok event -> mapEvent event
