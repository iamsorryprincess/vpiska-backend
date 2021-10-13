module Vpiska.Domain.CommandHandler

open System
open Vpiska.Domain.Commands

let private getService<'TService> (sp: IServiceProvider) = sp.GetService(typeof<'TService>) :?> 'TService

let private handleCreateUser (sp: IServiceProvider) (args: CreateUserArgs) =
    let db = getService<IUserRepository> sp
    let auth = getService<IAuth> sp
    Logic.createUser db.CheckInfo db.Create auth.HashPassword auth.GetToken args
    
let private handleLoginUser (sp: IServiceProvider) (args: LoginUserArgs) =
    let db = getService<IUserRepository> sp
    let auth = getService<IAuth> sp
    Logic.loginUser db.GetUserByPhone auth.CheckPassword auth.GetToken args
    
let private handleSetCode (sp: IServiceProvider) (args: CodeArgs) =
    let db = getService<IUserRepository> sp
    let notification = getService<INotificationService> sp
    Logic.setVerificationCode db.SetVerificationCode notification.SendVerificationCode args
    
let private handleCheckCode (sp: IServiceProvider) (args: CheckCodeArgs) =
    let db = getService<IUserRepository> sp
    let auth = getService<IAuth> sp
    Logic.checkVerificationCode db.GetUserByPhone auth.GetToken args
    
let private handleChangePassword (sp: IServiceProvider) (args: ChangePasswordArgs) =
    let db = getService<IUserRepository> sp
    let auth = getService<IAuth> sp
    Logic.changePassword db.ChangePassword auth.HashPassword args
    
let private handleUpdateUser (sp: IServiceProvider) (args: UpdateUserArgs) =
    let db = getService<IUserRepository> sp
    let fileStorage = getService<IFileStorage> sp
    Logic.updateUser db.GetById fileStorage.UploadFile db.Update args
    
let handle (sp: IServiceProvider) (command: Command) =
    match command with
    | CreateUser args -> handleCreateUser sp args
    | LoginUser args -> handleLoginUser sp args
    | SetCode args -> handleSetCode sp args
    | CheckCode args -> handleCheckCode sp args
    | ChangePassword args -> handleChangePassword sp args
    | UpdateUser args -> handleUpdateUser sp args
