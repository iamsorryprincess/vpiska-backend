namespace Vpiska.Api.Control.CommandHandlers

open Google.Cloud.Storage.V1
open Orleans
open Vpiska.Infrastructure.Orleans.Interfaces
open Vpiska.Api.Control.Infrastructure.Event
open Vpiska.Api.Control.Infrastructure.Firebase
open Vpiska.Domain.Event.Commands
open Vpiska.Domain.Event.Logic

type EventCommandsPersistence =
    { ClusterClient: IClusterClient
      StreamProducer: IStreamProducer
      AreaSettings: AreaSettings
      FirebaseStorage: StorageClient
      FirebaseSettings: FirebaseSettings }

module EventCommandHandler =
    
    let private publish (producer: IStreamProducer) eventId event = producer.Produce(eventId, event)
    
    let handle persistence command =
        match command with
        | CreateEvent args ->
            let checkOwner = Cluster.checkOwner persistence.ClusterClient
            let createEvent = Cluster.createEvent persistence.ClusterClient
            let checkArea area = persistence.AreaSettings.Areas |> Array.contains area
            CommandsLogic.createEvent checkArea checkOwner createEvent args
        | CloseEvent args ->
            let checkEvent = Cluster.checkEvent persistence.ClusterClient
            let checkOwnership = Cluster.checkOwnership persistence.ClusterClient
            let publish = publish persistence.StreamProducer
            let closeEvent = Cluster.closeEvent persistence.ClusterClient
            CommandsLogic.closeEvent checkEvent checkOwnership publish closeEvent args
        | AddMedia args ->
            let checkEvent = Cluster.checkEvent persistence.ClusterClient
            let checkOwnership = Cluster.checkOwnership persistence.ClusterClient
            let addMedia = Cluster.addMedia persistence.ClusterClient
            let uploadFile = FileStorage.uploadFile persistence.FirebaseStorage persistence.FirebaseSettings.BucketName
            CommandsLogic.addMedia checkEvent checkOwnership addMedia uploadFile args
        | RemoveMedia args ->
            let checkEvent = Cluster.checkEvent persistence.ClusterClient
            let checkOwnership = Cluster.checkOwnership persistence.ClusterClient
            let removeMedia = Cluster.removeMedia persistence.ClusterClient
            let deleteFile = FileStorage.deleteFile persistence.FirebaseStorage persistence.FirebaseSettings.BucketName
            CommandsLogic.removeMedia checkEvent checkOwnership removeMedia deleteFile args
