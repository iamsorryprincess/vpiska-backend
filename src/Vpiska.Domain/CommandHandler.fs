module Vpiska.Domain.CommandHandler

open System
open Vpiska.Domain.Commands

let private handleCreateUser (sp: IServiceProvider) (args: CreateUserArgs) =
    let db = sp.GetService(typeof<IUserRepository>) :?> IUserRepository
    let auth = sp.GetService(typeof<IAuth>) :?> IAuth
    Logic.createUser db.CheckInfo db.Create auth.HashPassword auth.GetToken args
    
let private handleLoginUser (sp: IServiceProvider) (args: LoginUserArgs) =
    let db = sp.GetService(typeof<IUserRepository>) :?> IUserRepository
    let auth = sp.GetService(typeof<IAuth>) :?> IAuth
    Logic.loginUser db.GetUserByPhone auth.CheckPassword auth.GetToken args
    
let private handleSetCode (sp: IServiceProvider) (args: CodeArgs) =
    let db = sp.GetService(typeof<IUserRepository>) :?> IUserRepository
    let notification = sp.GetService(typeof<INotificationService>) :?> INotificationService
    Logic.setVerificationCode db.SetVerificationCode notification.SendVerificationCode args
    
let private handleCheckCode (sp: IServiceProvider) (args: CheckCodeArgs) =
    let db = sp.GetService(typeof<IUserRepository>) :?> IUserRepository
    let auth = sp.GetService(typeof<IAuth>) :?> IAuth
    Logic.checkVerificationCode db.GetUserByPhone auth.GetToken args
    
let private handleChangePassword (sp: IServiceProvider) (args: ChangePasswordArgs) =
    let db = sp.GetService(typeof<IUserRepository>) :?> IUserRepository
    let auth = sp.GetService(typeof<IAuth>) :?> IAuth
    Logic.changePassword db.ChangePassword auth.HashPassword args
    
let private handleUpdateUser (sp: IServiceProvider) (args: UpdateUserArgs) =
    let db = sp.GetService(typeof<IUserRepository>) :?> IUserRepository
    let fileStorage = sp.GetService(typeof<IFileStorage>) :?> IFileStorage
    Logic.updateUser db.GetById fileStorage.UploadFile db.Update args
    
let handle (sp: IServiceProvider) (command: Command) =
    match command with
    | CreateUser args -> handleCreateUser sp args
    | LoginUser args -> handleLoginUser sp args
    | SetCode args -> handleSetCode sp args
    | CheckCode args -> handleCheckCode sp args
    | ChangePassword args -> handleChangePassword sp args
    | UpdateUser args -> handleUpdateUser sp args
