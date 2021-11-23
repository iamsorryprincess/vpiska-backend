namespace Vpiska.Application.Event

open Google.Cloud.Storage.V1
open Orleans
open Vpiska.Application.Firebase
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Logic
open Vpiska.Infrastructure.Orleans.Interfaces

type CommandHandler(clusterClient: IClusterClient,
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
        | Subscribe args -> CommandsLogic.subscribe streamProducer.TrySubscribe args
        | Unsubscribe args -> CommandsLogic.unsubscribe streamProducer.TryUnsubscribe args
        | LogUserInChat args ->
            let getUsers = EventClusterClient.getUsers clusterClient
            let addUser = EventClusterClient.addUser clusterClient
            CommandsLogic.connectUserToChat checkEvent getUsers addUser publish args
        | LogoutUserFromChat args ->
            let removeUser = EventClusterClient.removeUser clusterClient
            CommandsLogic.disconnectUserFromChat checkEvent removeUser publish args
        | SendChatMessage args ->
            let addMessage = EventClusterClient.addMessage clusterClient
            CommandsLogic.sendChatMessage checkEvent addMessage publish args
        | AddMedia args ->
            let addMedia = EventClusterClient.addMedia clusterClient
            let uploadFile = Storage.uploadFile firebaseClient firebaseSettings.BucketName
            CommandsLogic.addMedia checkEvent checkOwnership addMedia uploadFile args
        | RemoveMedia args ->
            let removeMedia = EventClusterClient.removeMedia clusterClient
            let deleteFile = Storage.deleteFile firebaseClient firebaseSettings.BucketName
            CommandsLogic.removeMedia checkEvent checkOwnership removeMedia deleteFile args
    
    member _.Handle command = handle command
