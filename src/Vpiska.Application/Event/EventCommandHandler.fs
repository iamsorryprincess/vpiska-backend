namespace Vpiska.Application.Event

open Google.Cloud.Storage.V1
open Orleans
open Vpiska.Application.Firebase
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Commands
open Vpiska.Domain.Event.Logic
open Vpiska.Infrastructure.Orleans.Interfaces

type EventCommandHandler(clusterClient: IClusterClient,
                         streamProducer: IStreamProducer,
                         areaSettings: EventsCluster.AreaSettings,
                         firebaseClient: StorageClient,
                         firebaseSettings: Storage.FirebaseSettings) =
    
    let checkArea (area: string) = areaSettings.Areas |> Array.contains area
    
    let checkEvent = EventsCluster.checkEvent clusterClient
    
    let checkOwnership = EventsCluster.checkOwnership clusterClient

    let publish (eventId: string) (event: DomainEvent) = streamProducer.Produce(eventId, event)
    
    let handle command =
        match command with
        | CreateEvent args ->
            let checkOwner = EventsCluster.checkOwner clusterClient
            let createEvent = EventsCluster.createEvent clusterClient
            CommandsLogic.createEvent checkArea checkOwner createEvent args
        | CloseEvent args ->
            let closeEvent = EventsCluster.closeEvent clusterClient
            CommandsLogic.closeEvent checkEvent checkOwnership publish closeEvent args
        | AddMedia args ->
            let addMedia = EventsCluster.addMedia clusterClient
            let uploadFile = Storage.uploadFile firebaseClient firebaseSettings.BucketName
            CommandsLogic.addMedia checkEvent checkOwnership addMedia uploadFile args
        | RemoveMedia args ->
            let removeMedia = EventsCluster.removeMedia clusterClient
            let deleteFile = Storage.deleteFile firebaseClient firebaseSettings.BucketName
            CommandsLogic.removeMedia checkEvent checkOwnership removeMedia deleteFile args
    
    member _.Handle command = handle command
