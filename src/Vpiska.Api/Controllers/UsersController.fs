namespace Vpiska.Api.Controllers

open System
open System.IO
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks
open Vpiska.Api
open Vpiska.Domain
open Vpiska.Domain.Commands
open Vpiska.Domain.Responses

[<CLIMutable>]
type UpdateUserRequest =
    { Id: string
      Name: string
      Phone: string
      Image: IFormFile }

[<Route "api/users">]
type UsersController(sp: IServiceProvider) =
    inherit ControllerBase()
    
    let handle command =
        task {
            let! result = CommandHandler.handle sp command
            return Http.mapResponse result
        }
        
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
    
    [<HttpPost "create">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response<LoginResponse>>, 200)>]
    member _.Create([<FromBody>] args: CreateUserArgs) = args |> Command.CreateUser |> handle
    
    [<HttpPost "login">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response<LoginResponse>>, 200)>]
    member _.Login([<FromBody>] args: LoginUserArgs) = args |> Command.LoginUser |> handle
    
    [<HttpPost "code/set">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response>, 200)>]
    member _.SetCode([<FromBody>] args: CodeArgs) = args |> Command.SetCode |> handle
    
    [<HttpPost "code/check">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response<LoginResponse>>, 200)>]
    member _.CheckVerificationCode([<FromBody>] args: CheckCodeArgs) = args |> Command.CheckCode |> handle
    
    [<Authorize>]
    [<HttpPost "password">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response>, 200)>]
    member _.ChangePassword([<FromBody>] args: ChangePasswordArgs) = args |> Command.ChangePassword |> handle
    
    [<Authorize>]
    [<HttpPost "update">]
    [<Produces("application/json")>]
    [<Consumes("multipart/form-data")>]
    [<ProducesResponseType(typeof<Response>, 200)>]
    member _.Update([<FromForm>] request: UpdateUserRequest) =
        task {
            let! args = mapUpdateRequest request
            return! args |> Command.UpdateUser |> handle
        }
    