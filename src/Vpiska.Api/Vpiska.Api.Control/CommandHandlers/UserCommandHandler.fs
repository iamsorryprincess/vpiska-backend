namespace Vpiska.Api.Control.CommandHandlers

open System
open FirebaseAdmin.Messaging
open Google.Cloud.Storage.V1
open MongoDB.Driver
open Vpiska.Api.Control.Infrastructure.Firebase
open Vpiska.Api.Control.Infrastructure.User
open Vpiska.Domain.User
open Vpiska.Infrastructure.Jwt

type UserPersistence =
    { MongoClient: MongoClient
      MongoSettings: MongoSettings
      FirebaseStorage: StorageClient
      FirebaseSettings: FirebaseSettings }

module UserCommandHandler =
    
    let private generateId () = Guid.NewGuid().ToString("N")
    
    let handle persistence command =
        match command with
        | Create args ->
            let checkUser = Database.getUserByPhoneAndName persistence.MongoClient persistence.MongoSettings.DatabaseName
            let createUser = Database.createUser persistence.MongoClient persistence.MongoSettings.DatabaseName
            let userId = generateId ()
            Logic.createUser checkUser createUser Security.hashPassword Jwt.encodeJwt userId args
        | Login args ->
            let getUserByPhone = Database.getUserByPhone persistence.MongoClient persistence.MongoSettings.DatabaseName
            Logic.loginUser getUserByPhone Security.checkPassword Jwt.encodeJwt args
        | SetCode args ->
            let setCode = Database.setCode persistence.MongoClient persistence.MongoSettings.DatabaseName
            let pushNotification = CloudMessaging.pushNotification FirebaseMessaging.DefaultInstance
            Logic.setVerificationCode setCode pushNotification CodeGenerator.generate args
        | CheckCode args ->
            let getUserByPhone = Database.getUserByPhone persistence.MongoClient persistence.MongoSettings.DatabaseName
            Logic.checkVerificationCode getUserByPhone Jwt.encodeJwt args
        | ChangePassword args ->
            let changePassword = Database.changePassword persistence.MongoClient persistence.MongoSettings.DatabaseName
            Logic.changePassword Security.hashPassword changePassword args
        | Update args ->
            let getUser = Database.getUserById persistence.MongoClient persistence.MongoSettings.DatabaseName
            let checkPhone = Database.isPhoneExist persistence.MongoClient persistence.MongoSettings.DatabaseName
            let checkName = Database.isNameExist persistence.MongoClient persistence.MongoSettings.DatabaseName
            let uploadFile = FileStorage.uploadFile persistence.FirebaseStorage persistence.FirebaseSettings.BucketName
            let updateUser = Database.updateUser persistence.MongoClient persistence.MongoSettings.DatabaseName
            let imageId = generateId ()
            Logic.updateUser getUser checkPhone checkName uploadFile updateUser imageId args
