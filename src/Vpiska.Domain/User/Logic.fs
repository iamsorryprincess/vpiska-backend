module Vpiska.Domain.User.Logic

open System
open System.Threading.Tasks
open FSharp.Control.Tasks

let private phoneCode: PhoneCode = "+7"

let private isNull o = Object.ReferenceEquals(o, null)

type GenerateUserId = unit -> UserId
type GenerateImageId = unit -> ImageId
type GenerateCode = unit -> VerificationCode

type CheckUser = Phone -> Username -> Task<CheckPhoneNameResult>
type CreateUser = User -> Task<UserId>
type UpdateUser = UserId -> Username -> Phone -> ImageId -> Task<bool>
type HashPassword = string -> HashedPassword
type EncodeToken = UserId -> Username -> ImageId -> string

type GetUserByPhone = Phone -> Task<User>
type GetUserById = UserId -> Task<User>
type CheckPassword = string -> HashedPassword -> bool
type ChangePassword = UserId -> HashedPassword -> Task<bool>
type CheckName = Username -> Task<bool>
type CheckPhone = Phone -> Task<bool>

type SetCode = Phone -> VerificationCode -> Task<bool>
type PushNotification = VerificationCode -> string -> Task<unit>
type ContentType = string
type UploadFile = ImageId -> byte[] -> ContentType -> Task<ImageId>

let createUser
  (check: CheckUser)
  (create: CreateUser)
  (hashPassword: HashPassword)
  (encodeToken: EncodeToken)
  (generateId: GenerateUserId)
  (args: CreateUserArgs) =
  task {
    match UserValidation.validateCreateUserArgs args with
      | Error errors -> return errors |> Errors.fromValidation
      | Ok _ ->
        match! check args.Phone args.Name with
        | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = false } ->
          return DomainError.PhoneAlreadyUse |> Errors.fromDomain
        | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = true } ->
          return DomainError.NameAlreadyUse |> Errors.fromDomain
        | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = true } ->
          return [|DomainError.NameAlreadyUse; DomainError.PhoneAlreadyUse|] |> Errors.fromDomainArr
        | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = false } ->
          let password = hashPassword args.Password
          let id = generateId ()
          let user = args.toDomain id phoneCode password
          let! userId = create user
          let token = encodeToken userId user.Name user.ImageId
          return DomainEvent.UserLogged { UserId = userId; AccessToken = token } |> Ok
  }

let loginUser
  (getUser: GetUserByPhone)
  (checkPassword: CheckPassword)
  (encodeToken: EncodeToken)
  (args: LoginUserArgs) =
  task {
    match UserValidation.validateLoginUserArgs args with
    | Error errors -> return errors |> Errors.fromValidation
    | Ok _ ->
      let! user = getUser args.Phone
      if isNull user then
        return DomainError.UserByPhoneNotFound |> Errors.fromDomain
      else
        match checkPassword args.Password user.Password with
        | false -> return DomainError.InvalidPassword |> Errors.fromDomain
        | true ->
          let token = encodeToken user.Id user.Name user.ImageId
          return DomainEvent.UserLogged { UserId = user.Id; AccessToken = token } |> Ok
  }
  
let setVerificationCode
  (setCode: SetCode)
  (pushNotification: PushNotification)
  (generateCode: GenerateCode)
  (args: CodeArgs) =
  task {
    match UserValidation.validateCodeArgs args with
    | Error errors -> return errors |> Errors.fromValidation
    | Ok _ ->
      let code = generateCode ()
      match! setCode args.Phone code with
      | false -> return DomainError.UserByPhoneNotFound |> Errors.fromDomain
      | true ->
        do! pushNotification code args.Token
        return DomainEvent.CodePushed |> Ok
  }
  
let checkVerificationCode
  (getUser: GetUserByPhone)
  (encodeToken: EncodeToken)
  (args: CheckCodeArgs) =
  task {
    match UserValidation.validateCheckCodeArgs args with
    | Error errors -> return errors |> Errors.fromValidation
    | Ok _ ->
      let! user = getUser args.Phone
      if isNull user then
        return DomainError.UserByPhoneNotFound |> Errors.fromDomain
      else
        if user.VerificationCode = args.Code.Value then
          let token = encodeToken user.Id user.Name user.ImageId
          return DomainEvent.UserLogged { UserId = user.Id; AccessToken = token } |> Ok
        else
          return DomainError.InvalidCode |> Errors.fromDomain
  }
  
let changePassword
  (hashPassword: HashPassword)
  (changePassword: ChangePassword)
  (args: ChangePasswordArgs) =
  task {
    match UserValidation.validateChangePasswordArgs args with
    | Error errors -> return errors |> Errors.fromValidation
    | Ok _ ->
      let newPassword = hashPassword args.Password
      match! changePassword args.Id newPassword with
      | false -> return DomainError.UserNotFound |> Errors.fromDomain
      | true -> return DomainEvent.PasswordChanged |> Ok
  }

let updateUser
  (getUser: GetUserById)
  (checkPhone: CheckPhone)
  (checkName: CheckName)
  (uploadFile: UploadFile)
  (updateUser: UpdateUser)
  (generateId: GenerateImageId)
  (args: UpdateUserArgs) =
  task {
    match UserValidation.validateUpdateUserArgs args with
    | Error errors -> return errors |> Errors.fromValidation
    | Ok _ ->
      let! user = getUser args.Id
      if isNull user then
        return [|DomainError.UserNotFound |> AppError.create|] |> Error
      else
        match! Task.WhenAll [checkPhone args.Phone; checkName args.Name] with
        | [|true; true|] -> return [|DomainError.PhoneAlreadyUse; DomainError.NameAlreadyUse|] |> Errors.fromDomainArr
        | [|true; false|] -> return DomainError.PhoneAlreadyUse |> Errors.fromDomain
        | [|false; true|] -> return DomainError.NameAlreadyUse |> Errors.fromDomain
        | _ ->
          if isNull args.ImageData then
            let! _ = updateUser args.Id args.Name args.Phone user.ImageId
            return DomainEvent.UserUpdated |> Ok
          else
            let! imageId = if String.IsNullOrWhiteSpace user.ImageId
                           then uploadFile (generateId ()) args.ImageData args.ContentType
                           else uploadFile user.ImageId args.ImageData args.ContentType
            let! _ = updateUser args.Id args.Name args.Phone imageId
            return DomainEvent.UserUpdated |> Ok
  }
