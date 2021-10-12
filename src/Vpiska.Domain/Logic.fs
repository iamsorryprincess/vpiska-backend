module Vpiska.Domain.Logic

open System
open System.IO
open System.Threading.Tasks
open FSharp.Control.Tasks
open Vpiska.Domain.Models
open Vpiska.Domain.Errors
open Vpiska.Domain.Commands
open Vpiska.Domain.Responses
    
let private phoneCode = "+7"

let private mapCreateUserArgs (args: CreateUserArgs): User =
    { Id = Guid.NewGuid().ToString(); Name = args.Name; Phone = args.Phone
      PhoneCode = phoneCode; ImageUrl = null; Password = args.Password; VerificationCode = 0 }

let createUser
    (dbCheck: string -> string -> Task<CheckPhoneNameResult>)
    (dbCreate: User -> Task<string>)
    (hashPassword: string -> string)
    (encodeToken: string -> string -> string -> string)
    (request: CreateUserArgs) =
    task {
        match Validation.validateCreateUserArgs request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            match! dbCheck request.Phone request.Name |> Async.AwaitTask with
            | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = false } ->
                return [|DomainError.PhoneAlreadyUse |> AppError.create|] |> Error
            | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = true } ->
                return [|DomainError.NameAlreadyUse |> AppError.create|] |> Error
            | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = true } ->
                return [|DomainError.NameAlreadyUse |> AppError.create; DomainError.PhoneAlreadyUse |> AppError.create|] |> Error
            | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = false } ->
                let model = mapCreateUserArgs request
                let user = { model with Password = hashPassword model.Password }
                let! userId = dbCreate user |> Async.AwaitTask
                let token = encodeToken userId user.Name user.ImageUrl
                return DomainResponse.Login { UserId = userId; AccessToken = token } |> Ok
    }
    
let loginUser
    (dbGetUser: string -> Task<User voption>)
    (checkPassword: string -> string -> bool)
    (encodeToken: string -> string -> string -> string)
    (request: LoginUserArgs) =
    task {
        match Validation.validateLoginUserArgs request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            match! dbGetUser request.Phone with
            | ValueNone -> return [|DomainError.UserByPhoneNotFound |> AppError.create|] |> Error
            | ValueSome user ->
                if checkPassword user.Password request.Password then
                    let token = encodeToken user.Id user.Name user.ImageUrl
                    return DomainResponse.Login { UserId = user.Id; AccessToken = token } |> Ok
                else
                    return [|DomainError.InvalidPassword |> AppError.create|] |> Error
    }
    
let private random = Random()
let private generateCode () = random.Next(111111, 777777)
    
let setVerificationCode
    (dbSetCode: string -> int -> Task<bool>)
    (notify: int -> Task)
    (request: CodeArgs) =
    task {
        match Validation.validateCodeArgs request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            let code = generateCode ()
            match! dbSetCode request.Phone code with
            | false -> return [|DomainError.UserByPhoneNotFound |> AppError.create|] |> Error
            | true ->
                do! notify code
                return DomainResponse.Code |> Ok
    }
    
let checkVerificationCode
    (dbGetUser: string -> Task<User voption>)
    (encodeToken: string -> string -> string -> string)
    (request: CheckCodeArgs) =
    task {
        match Validation.validateCheckCodeArgs request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            match! dbGetUser request.Phone with
            | ValueNone -> return [|DomainError.UserByPhoneNotFound |> AppError.create|] |> Error
            | ValueSome user ->
                if user.VerificationCode = request.Code.Value then
                    let token = encodeToken user.Id user.Name user.ImageUrl
                    return DomainResponse.Login { UserId = user.Id; AccessToken = token } |> Ok
                else
                    return [|DomainError.InvalidCode |> AppError.create|] |> Error
    }
    
let changePassword
    (dbChangePassword: string -> string -> Task<bool>)
    (hashPassword: string -> string)
    (request: ChangePasswordArgs) =
    task {
        match Validation.validateChangePassword request with
        | Error errors -> return errors |> Array.map AppError.create |> Error
        | Ok _ ->
            let newPassword = hashPassword request.Password
            match! dbChangePassword request.Id newPassword with
            | false -> return [|DomainError.UserNotFound |> AppError.create|] |> Error
            | true -> return DomainResponse.PasswordChanged |> Ok
    }
   
let updateUser
    (dbGetUser: string -> Task<User voption>)
    (uploadFile: string -> string -> Stream -> Task<string>)
    (dbUpdateUser: string -> string -> string -> string -> Task<bool>)
    (request: UpdateUserArgs) =
    task {
        match! dbGetUser request.Id with
        | ValueNone -> return [|DomainError.UserNotFound |> AppError.create|] |> Error
        | ValueSome user ->
            match request.ImageData with
            | ValueSome data ->
                let! imageId = if String.IsNullOrWhiteSpace user.ImageUrl
                               then uploadFile (Guid.NewGuid().ToString()) request.ContentType (new MemoryStream(data))
                               else uploadFile user.ImageUrl request.ContentType (new MemoryStream(data))
                let! _ = dbUpdateUser request.Id request.Name request.Phone imageId
                return DomainResponse.UserUpdated |> Ok
            | ValueNone ->
                let! _ = dbUpdateUser request.Id request.Name request.Phone user.ImageUrl
                return DomainResponse.UserUpdated |> Ok
    }
