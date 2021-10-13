namespace Vpiska.Api.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks
open Vpiska.Api
open Vpiska.Domain
open Vpiska.Domain.Commands
open Vpiska.Domain.Responses
open Vpiska.Firebase
open Vpiska.JwtAuth
open Vpiska.Mongo

[<Route "api/users">]
type UsersController() =
    inherit ControllerBase()
    
    [<HttpPost "create">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response<LoginResponse>>, 200)>]
    member _.Create([<FromServices>] db: MongoUserRepository, [<FromServices>] auth: JwtAuthService, [<FromBody>] args: CreateUserArgs) =
        task {
            let! response = UserDomain.createUser db.CheckInfo db.Create auth.HashPassword auth.GetToken args
            return Http.mapResponse response
        }
    
    [<HttpPost "login">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response<LoginResponse>>, 200)>]
    member _.Login([<FromServices>] db: MongoUserRepository, [<FromServices>] auth: JwtAuthService, [<FromBody>] args: LoginUserArgs) =
        task {
            let! response = UserDomain.loginUser db.GetUserByPhone auth.CheckPassword auth.GetToken args
            return Http.mapResponse response
        }
    
    [<HttpPost "code/set">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response>, 200)>]
    member _.SetCode([<FromServices>] db: MongoUserRepository, [<FromServices>] notification: FirebaseCloudMessagingService, [<FromBody>] args: CodeArgs) =
        task {
            let! response = UserDomain.setVerificationCode db.SetVerificationCode notification.SendVerificationCode args
            return Http.mapResponse response
        }
    
    [<HttpPost "code/check">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response<LoginResponse>>, 200)>]
    member _.CheckVerificationCode([<FromServices>] db: MongoUserRepository, [<FromServices>] auth: JwtAuthService, [<FromBody>] args: CheckCodeArgs) =
        task {
            let! response = UserDomain.checkVerificationCode db.GetUserByPhone auth.GetToken args
            return Http.mapResponse response
        }
    
    [<Authorize>]
    [<HttpPost "password">]
    [<Produces("application/json")>]
    [<Consumes("application/json")>]
    [<ProducesResponseType(typeof<Response>, 200)>]
    member _.ChangePassword([<FromServices>] db: MongoUserRepository, [<FromServices>] auth: JwtAuthService, [<FromBody>] args: ChangePasswordArgs) =
        task {
            let! response = UserDomain.changePassword db.ChangePassword auth.HashPassword args
            return Http.mapResponse response
        }
    
    [<Authorize>]
    [<HttpPost "update">]
    [<Produces("application/json")>]
    [<Consumes("multipart/form-data")>]
    [<ProducesResponseType(typeof<Response>, 200)>]
    member _.Update([<FromServices>] db: MongoUserRepository, [<FromServices>] fileStorage: FirebaseFileStorage, [<FromForm>] request: UpdateUserRequest) =
        task {
            let! args = Http.mapUpdateRequest request
            let! response = UserDomain.updateUser db.GetById fileStorage.UploadFile db.Update args
            return Http.mapResponse response
        }
    