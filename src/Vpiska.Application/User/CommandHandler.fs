namespace Vpiska.Application.User

open System
open Google.Cloud.Storage.V1
open MongoDB.Driver
open Vpiska.Application.Firebase
open Vpiska.Domain.User

type CommandHandler(mongoClient: MongoClient,
                    mongoSetting: Database.MongoSettings,
                    firebaseStorageClient: StorageClient,
                    firebaseSettings: Storage.FirebaseSettings) =
    
    let getUserByPhone = Database.getUserByPhone mongoClient mongoSetting.DatabaseName
    
    let generateId () = Guid.NewGuid().ToString("N")
    
    let handle command =
        match command with
        | Create args ->
            let checkUser = Database.getUserByPhoneAndName mongoClient mongoSetting.DatabaseName
            let createUser = Database.createUser mongoClient mongoSetting.DatabaseName
            Logic.createUser checkUser createUser Security.hashPassword Jwt.encodeJwt generateId args
        | Login args -> Logic.loginUser getUserByPhone Security.checkPassword Jwt.encodeJwt args
        | SetCode args ->
            let setCode = Database.setCode mongoClient mongoSetting.DatabaseName
            Logic.setVerificationCode setCode CloudMessaging.pushNotification CodeGenerator.generate args
        | CheckCode args -> Logic.checkVerificationCode getUserByPhone Jwt.encodeJwt args
        | ChangePassword args ->
            let changePassword = Database.changePassword mongoClient mongoSetting.DatabaseName
            Logic.changePassword Security.hashPassword changePassword args
        | Update args ->
            let getUser = Database.getUserById mongoClient mongoSetting.DatabaseName
            let checkPhone = Database.isPhoneExist mongoClient mongoSetting.DatabaseName
            let checkName = Database.isNameExist mongoClient mongoSetting.DatabaseName
            let uploadFile = Storage.uploadFile firebaseStorageClient firebaseSettings.BucketName
            let updateUser = Database.updateUser mongoClient mongoSetting.DatabaseName
            Logic.updateUser getUser checkPhone checkName uploadFile updateUser generateId args
    
    member _.Handle command = handle command
