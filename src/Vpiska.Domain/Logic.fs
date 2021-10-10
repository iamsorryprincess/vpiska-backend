module Vpiska.Domain.Logic

open System
open System.IO
open System.Threading.Tasks
open FSharp.Control.Tasks
open Vpiska.Domain.Models
open Vpiska.Domain.Errors
open Vpiska.Domain.Commands
open Vpiska.Domain.Responses

type Validate<'TRequest> = 'TRequest -> Result<'TRequest, ValidationError[]>
type Map<'TRequest, 'TResponse> = 'TRequest -> 'TResponse

type DbGetUserById = string -> Task<User voption>
type DbCheckInfo = string -> string -> Task<CheckPhoneNameResult>
type DbCreateUser = User -> Task<string>
type DbUpdateUser = string -> string -> string -> string -> Task<bool>
type DbGetUserByPhone = string -> Task<User voption>
type DbSetCode = string -> int -> Task<bool>
type DbChangePassword = string -> string -> Task<bool>

type SecHashPassword = string -> string
type SecCheckPassword = string -> string -> bool
type SecGetToken = string -> string -> string -> string

type UploadFile = string -> string -> Stream -> Task<string>
type NotifyCode = int -> Task

let private random = Random()
let private generateCode = random.Next(111111, 777777)

let createUser
    (validate: Validate<CreateUserArgs>)
    (map: Map<CreateUserArgs, User>)
    (dbCheck: DbCheckInfo)
    (dbCreate: DbCreateUser)
    (hashPassword: SecHashPassword)
    (getToken: SecGetToken)
    (request: CreateUserArgs) =
    task {
        match validate request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            match! dbCheck request.Name request.Phone |> Async.AwaitTask with
            | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = false } ->
                return [|DomainError.PhoneAlreadyUse |> AppError.create|] |> Error
            | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = true } ->
                return [|DomainError.NameAlreadyUse |> AppError.create|] |> Error
            | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = true } ->
                return [|DomainError.NameAlreadyUse |> AppError.create; DomainError.PhoneAlreadyUse |> AppError.create|] |> Error
            | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = false } ->
                let model = map request
                let user = { model with Password = hashPassword model.Password }
                let! userId = dbCreate user |> Async.AwaitTask
                let token = getToken userId user.Name user.ImageUrl
                return DomainResponse.Login { UserId = userId; AccessToken = token } |> Ok
    }
    
let loginUser
    (validate: Validate<LoginUserArgs>)
    (dbGetUser: DbGetUserByPhone)
    (checkPassword: SecCheckPassword)
    (getToken: SecGetToken)
    (request: LoginUserArgs) =
    task {
        match validate request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            match! dbGetUser request.Phone with
            | ValueNone -> return [|DomainError.UserByPhoneNotFound |> AppError.create|] |> Error
            | ValueSome user ->
                if checkPassword user.Password request.Password then
                    let token = getToken user.Id user.Name user.ImageUrl
                    return DomainResponse.Login { UserId = user.Id; AccessToken = token } |> Ok
                else
                    return [|DomainError.InvalidPassword |> AppError.create|] |> Error
    }
    
let setVerificationCode
    (validate: Validate<CodeArgs>)
    (dbSetCode: DbSetCode)
    (notify: NotifyCode)
    (request: CodeArgs) =
    task {
        match validate request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            let code = generateCode
            match! dbSetCode request.Phone code with
            | false -> return [|DomainError.UserByPhoneNotFound |> AppError.create|] |> Error
            | true ->
                do! notify code
                return DomainResponse.Code |> Ok
    }
    
let checkVerificationCode
    (validate: Validate<CheckCodeArgs>)
    (dbGetUser: DbGetUserByPhone)
    (getToken: SecGetToken)
    (request: CheckCodeArgs) =
    task {
        match validate request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            match! dbGetUser request.Phone with
            | ValueNone -> return [|DomainError.UserByPhoneNotFound |> AppError.create|] |> Error
            | ValueSome user ->
                if user.VerificationCode = request.Code.Value then
                    let token = getToken user.Id user.Name user.ImageUrl
                    return DomainResponse.Login { UserId = user.Id; AccessToken = token } |> Ok
                else
                    return [|DomainError.InvalidCode |> AppError.create|] |> Error
    }
    
let changePassword
    (validate: Validate<ChangePasswordArgs>)
    (dbChangePassword: DbChangePassword)
    (hashPassword: SecHashPassword)
    (request: ChangePasswordArgs) =
    task {
        match validate request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            let newPassword = hashPassword request.Password
            match! dbChangePassword request.Id newPassword with
            | false -> return [|DomainError.UserNotFound |> AppError.create|] |> Error
            | true -> return DomainResponse.PasswordChanged |> Ok
    }
    
let updateUser (dbGetUser: DbGetUserById) (uploadFile: UploadFile) (dbUpdateUser: DbUpdateUser) (request: UpdateUserArgs) =
    task {
        match! dbGetUser request.Id with
        | ValueNone -> return [|DomainError.UserNotFound |> AppError.create|] |> Error
        | ValueSome user ->
            match request.ImageStream with
            | ValueSome stream ->
                if String.IsNullOrWhiteSpace user.ImageUrl then
                    let! imageId = uploadFile (Guid.NewGuid().ToString()) request.ContentType stream
                    let! _ = dbUpdateUser request.Id request.Name request.Phone imageId
                    return DomainResponse.UserUpdated |> Ok
                else
                    let! imageId = uploadFile user.ImageUrl request.ContentType stream
                    let! _ = dbUpdateUser request.Id request.Name request.Phone imageId
                    return DomainResponse.UserUpdated |> Ok
            | ValueNone ->
                let! _ = dbUpdateUser request.Id request.Name request.Phone user.ImageUrl
                return DomainResponse.UserUpdated |> Ok
    }
