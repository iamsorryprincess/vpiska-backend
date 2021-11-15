namespace Vpiska.Application.User

open Google.Cloud.Storage.V1
open MongoDB.Driver
open FSharp.Control.Tasks
open Vpiska.Domain.User
open Vpiska.Infrastructure.Firebase
open Vpiska.Infrastructure.Mongo
open Vpiska.Application.User.CommandHandler

type UserMobileHttpHandler(mongoClient: MongoClient, mongoSettings: MongoSettings,
                           storageClient: StorageClient, firebaseSettings: FirebaseSettings) =
    
    let persistence = { MongoClient = mongoClient; MongoSettings = mongoSettings
                        FirebaseStorageClient = storageClient; FirebaseSettings = firebaseSettings }
    
    member _.Handle command =
        task {
            let! result = handle persistence command
            return Http.mapToMobileResult result
        }
    
    member _.HandleUpdateUser (args: Vpiska.Application.User.UpdateUserArgs) =
        task {
            let! data = args.toCommandArgs()
            let command = Command.Update data
            let! result = handle persistence command
            return Http.mapToMobileResult result
        }
    