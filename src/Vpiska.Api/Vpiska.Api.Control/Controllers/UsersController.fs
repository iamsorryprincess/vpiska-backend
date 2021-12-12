namespace Vpiska.Api.Control.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks
open Vpiska.Api.Control.CommandHandlers
open Vpiska.Api.Control.Http
open Vpiska.Domain.User

[<Route "api/users">]
type UsersController() =
    inherit ControllerBase()
    
    [<HttpPost "create">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse<UserLoggedArgs>>, 200)>]
    member _.Create([<FromServices>] persistence, [<FromBody>] args) =
        task {
            let command = args |> Command.Create
            let! result = UserCommandHandler.handle persistence command
            return UserHttpResponse.fromEventResult result
        }
    
    [<HttpPost "login">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse<UserLoggedArgs>>, 200)>]
    member _.Login([<FromServices>] persistence, [<FromBody>] args) =
        task {
            let command = args |> Command.Login
            let! result = UserCommandHandler.handle persistence command
            return UserHttpResponse.fromEventResult result
        }
        
    [<HttpPost "code/set">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse>, 200)>]
    member _.SetCode([<FromServices>] persistence, [<FromBody>] args) =
        task {
            let command = args |> Command.SetCode
            let! result = UserCommandHandler.handle persistence command
            return UserHttpResponse.fromEventResult result
        }
        
    [<HttpPost "code/check">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse<UserLoggedArgs>>, 200)>]
    member _.CheckCode([<FromServices>] persistence, [<FromBody>] args) =
        task {
            let command = args |> Command.CheckCode
            let! result = UserCommandHandler.handle persistence command
            return UserHttpResponse.fromEventResult result
        }
    
    [<Authorize>]
    [<HttpPost "password/change">]
    [<Produces "application/json">]
    [<Consumes "application/json">]
    [<ProducesResponseType(typeof<ApiResponse>, 200)>]
    member _.ChangePassword([<FromServices>] persistence, [<FromBody>] args) =
        task {
            let command = args |> Command.ChangePassword
            let! result = UserCommandHandler.handle persistence command
            return UserHttpResponse.fromEventResult result
        }
        
    [<Authorize>]
    [<HttpPost "update">]
    [<Produces "application/json">]
    [<Consumes "multipart/form-data">]
    [<ProducesResponseType(typeof<ApiResponse>, 200)>]
    member _.UpdateUser([<FromServices>] persistence, [<FromForm>] args: Vpiska.Api.Control.Http.UpdateUserArgs) =
        task {
            let! command = args.toCommand ()
            let! result = UserCommandHandler.handle persistence command
            return UserHttpResponse.fromEventResult result
        }
