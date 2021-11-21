namespace Vpiska.Application.Event

open Google.Cloud.Storage.V1
open Orleans
open Vpiska.Application.Firebase
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Interfaces

type AreaSettings = { Areas: string[] }

type CommandHandler(clusterClient: IClusterClient,
                    streamProducer: IStreamProducer,
                    areaSettings: AreaSettings,
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
            Domain.createEvent checkArea checkOwner createEvent args
        | CloseEvent args ->
            let closeEvent = EventClusterClient.closeEvent clusterClient
            Domain.closeEvent checkEvent checkOwnership publish closeEvent args
        | Subscribe args -> Domain.subscribe streamProducer.TrySubscribe args
        | Unsubscribe args -> Domain.unsubscribe streamProducer.TryUnsubscribe args
        | LogUserInChat args ->
            let getUsers = EventClusterClient.getUsers clusterClient
            let addUser = EventClusterClient.addUser clusterClient
            Domain.connectUserToChat checkEvent getUsers addUser publish args
        | LogoutUserFromChat args ->
            let removeUser = EventClusterClient.removeUser clusterClient
            Domain.disconnectUserFromChat checkEvent removeUser publish args
        | SendChatMessage args ->
            let addMessage = EventClusterClient.addMessage clusterClient
            Domain.sendChatMessage checkEvent addMessage publish args
        | AddMedia args ->
            let addMedia = EventClusterClient.addMedia clusterClient
            let uploadFile = Storage.uploadFile firebaseClient firebaseSettings.BucketName
            Domain.addMedia checkEvent checkOwnership addMedia uploadFile args
        | RemoveMedia args ->
            let removeMedia = EventClusterClient.removeMedia clusterClient
            let deleteFile = Storage.deleteFile firebaseClient firebaseSettings.BucketName
            Domain.removeMedia checkEvent checkOwnership removeMedia deleteFile args
    
    member _.Handle command = handle command
