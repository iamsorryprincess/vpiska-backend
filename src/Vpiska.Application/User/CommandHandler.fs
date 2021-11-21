namespace Vpiska.Application.User

open Google.Cloud.Storage.V1
open MongoDB.Driver
open Vpiska.Application.Firebase
open Vpiska.Domain.User

type CommandHandler(mongoClient: MongoClient,
                    mongoSetting: Database.MongoSettings,
                    firebaseStorageClient: StorageClient,
                    firebaseSettings: Storage.FirebaseSettings) =
    
    let getUserByPhone = Database.getUserByPhone mongoClient mongoSetting.DatabaseName
    
    let handle command =
        match command with
        | Create args ->
            let checkUser = Database.getUserByPhoneAndName mongoClient mongoSetting.DatabaseName
            let createUser = Database.createUser mongoClient mongoSetting.DatabaseName
            Domain.createUser checkUser createUser Security.hashPassword Jwt.encodeJwt args
        | Login args -> Domain.loginUser getUserByPhone Security.checkPassword Jwt.encodeJwt args
        | SetCode args ->
            let setCode = Database.setCode mongoClient mongoSetting.DatabaseName
            Domain.setVerificationCode setCode CloudMessaging.pushNotification args
        | CheckCode args -> Domain.checkVerificationCode getUserByPhone Jwt.encodeJwt args
        | ChangePassword args ->
            let changePassword = Database.changePassword mongoClient mongoSetting.DatabaseName
            Domain.changePassword Security.hashPassword changePassword args
        | Update args ->
            let getUser = Database.getUserById mongoClient mongoSetting.DatabaseName
            let checkPhone = Database.isPhoneExist mongoClient mongoSetting.DatabaseName
            let checkName = Database.isNameExist mongoClient mongoSetting.DatabaseName
            let uploadFile = Storage.uploadFile firebaseStorageClient firebaseSettings.BucketName
            let updateUser = Database.updateUser mongoClient mongoSetting.DatabaseName
            Domain.updateUser getUser checkPhone checkName uploadFile updateUser args
    
    member _.Handle command = handle command
