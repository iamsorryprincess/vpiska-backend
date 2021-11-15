module Vpiska.Application.User.CommandHandler

open Google.Cloud.Storage.V1
open MongoDB.Driver
open Vpiska.Domain.User
open Vpiska.Infrastructure.Firebase
open Vpiska.Infrastructure.Jwt
open Vpiska.Infrastructure.Mongo

type UserPersistence =
    { MongoClient: MongoClient
      MongoSettings: MongoSettings
      FirebaseStorageClient: StorageClient
      FirebaseSettings: FirebaseSettings }
    
let private createUser (persistence: UserPersistence) =
    let checkUser = Database.getUserByPhoneAndName persistence.MongoClient persistence.MongoSettings.DatabaseName
    let createUser = Database.createUser persistence.MongoClient persistence.MongoSettings.DatabaseName
    Domain.createUser checkUser createUser Security.hashPassword Jwt.encodeJwt
    
let private loginUser (persistence: UserPersistence) =
    let getUser = Database.getUserByPhone persistence.MongoClient persistence.MongoSettings.DatabaseName
    Domain.loginUser getUser Security.checkPassword Jwt.encodeJwt
    
let private setCode (persistence: UserPersistence) =
    let setCode = Database.setCode persistence.MongoClient persistence.MongoSettings.DatabaseName
    Domain.setVerificationCode setCode CloudMessaging.pushNotification
    
let private checkCode (persistence: UserPersistence) =
    let getUser = Database.getUserByPhone persistence.MongoClient persistence.MongoSettings.DatabaseName
    Domain.checkVerificationCode getUser Jwt.encodeJwt
    
let private changePassword (persistence: UserPersistence) =
    let changePassword = Database.changePassword persistence.MongoClient persistence.MongoSettings.DatabaseName
    Domain.changePassword Security.hashPassword changePassword
    
let private updateUser (persistence: UserPersistence) =
    let getUser = Database.getUserById persistence.MongoClient persistence.MongoSettings.DatabaseName
    let checkPhone = Database.isPhoneExist persistence.MongoClient persistence.MongoSettings.DatabaseName
    let checkName = Database.isNameExist persistence.MongoClient persistence.MongoSettings.DatabaseName
    let uploadFile = Storage.uploadFile persistence.FirebaseStorageClient persistence.FirebaseSettings.BucketName
    let updateUser = Database.updateUser persistence.MongoClient persistence.MongoSettings.DatabaseName
    Domain.updateUser getUser checkPhone checkName uploadFile updateUser
    
let handle persistence command =
    match command with
    | Create args -> createUser persistence args
    | Login args -> loginUser persistence args
    | SetCode args -> setCode persistence args
    | CheckCode args -> checkCode persistence args
    | ChangePassword args -> changePassword persistence args
    | Update args -> updateUser persistence args
