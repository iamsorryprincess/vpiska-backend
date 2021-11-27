namespace Vpiska.Application.Event

open System
open Google.Cloud.Storage.V1
open Orleans
open Vpiska.Application.Firebase
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Logic
open Vpiska.Infrastructure.Orleans.Interfaces

type EventCommandHandler(clusterClient: IClusterClient,
                         streamProducer: IStreamProducer,
                         areaSettings: EventClusterClient.AreaSettings,
                         firebaseClient: StorageClient,
                         firebaseSettings: Storage.FirebaseSettings) =
    
    let checkArea (area: string) = areaSettings.Areas |> Array.contains area
    
    let checkEvent = EventClusterClient.checkEvent clusterClient
    
    let checkOwnership = EventClusterClient.checkOwnership clusterClient

    let publish (eventId: string) (event: DomainEvent) = streamProducer.Produce(eventId, event)
    
    let handle command =
        match command with
        | CreateEvent args ->
            let checkOwner = EventClusterClient.checkOwner clusterClient
            let createEvent = EventClusterClient.createEvent clusterClient
            CommandsLogic.createEvent checkArea checkOwner createEvent args
        | CloseEvent args ->
            let closeEvent = EventClusterClient.closeEvent clusterClient
            CommandsLogic.closeEvent checkEvent checkOwnership publish closeEvent args
        | AddMedia args ->
            let addMedia = EventClusterClient.addMedia clusterClient
            let uploadFile = Storage.uploadFile firebaseClient firebaseSettings.BucketName
            CommandsLogic.addMedia checkEvent checkOwnership addMedia uploadFile args
        | RemoveMedia args ->
            let removeMedia = EventClusterClient.removeMedia clusterClient
            let deleteFile = Storage.deleteFile firebaseClient firebaseSettings.BucketName
            CommandsLogic.removeMedia checkEvent checkOwnership removeMedia deleteFile args
        | _ -> raise(ArgumentException("unknown command"))
    
    member _.Handle command = handle command
