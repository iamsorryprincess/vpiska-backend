namespace Vpiska.Domain

open System
open Vpiska.Domain.Commands

type CommandHandler(serviceProvider: IServiceProvider) =
    
    let handleCreateUser (args: CreateUserArgs) =
        let db = serviceProvider.GetService(typeof<IUserRepository>) :?> IUserRepository
        let auth = serviceProvider.GetService(typeof<IAuth>) :?> IAuth
        Logic.createUser Validation.validateCreateUserArgs Mapping.mapCreateUserArgs db.CheckInfo db.Create auth.HashPassword auth.GetToken args
        
    let handleLoginUser (args: LoginUserArgs) =
        let db = serviceProvider.GetService(typeof<IUserRepository>) :?> IUserRepository
        let auth = serviceProvider.GetService(typeof<IAuth>) :?> IAuth
        Logic.loginUser Validation.validateLoginUserArgs db.GetUserByPhone auth.CheckPassword auth.GetToken args
        
    let handleSetCode (args: CodeArgs) =
        let db = serviceProvider.GetService(typeof<IUserRepository>) :?> IUserRepository
        let notify = serviceProvider.GetService(typeof<INotificationService>) :?> INotificationService
        Logic.setVerificationCode Validation.validateCodeArgs db.SetVerificationCode notify.SendVerificationCode args
        
    let handleCheckCode (args: CheckCodeArgs) =
        let db = serviceProvider.GetService(typeof<IUserRepository>) :?> IUserRepository
        let auth = serviceProvider.GetService(typeof<IAuth>) :?> IAuth
        Logic.checkVerificationCode Validation.validateCheckCodeArgs db.GetUserByPhone auth.GetToken args
        
    let handleChangePassword (args: ChangePasswordArgs) =
        let db = serviceProvider.GetService(typeof<IUserRepository>) :?> IUserRepository
        let auth = serviceProvider.GetService(typeof<IAuth>) :?> IAuth
        Logic.changePassword Validation.validateChangePassword db.ChangePassword auth.HashPassword args
        
    let handleUpdateUser (args: UpdateUserArgs) =
        let db = serviceProvider.GetService(typeof<IUserRepository>) :?> IUserRepository
        let fileStorage = serviceProvider.GetService(typeof<IFileStorage>) :?> IFileStorage
        Logic.updateUser db.GetById fileStorage.UploadFile db.Update args
    
    member _.Handle command =
        match command with
        | CreateUser args -> handleCreateUser args
        | LoginUser args -> handleLoginUser args
        | SetCode args -> handleSetCode args
        | CheckCode args -> handleCheckCode args
        | ChangePassword args -> handleChangePassword args
        | UpdateUser args -> handleUpdateUser args
