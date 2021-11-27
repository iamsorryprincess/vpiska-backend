namespace Vpiska.Application.Event

open System
open Orleans
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Logic
open Vpiska.Infrastructure.Orleans.Interfaces

type ChatCommandHandler(clusterClient: IClusterClient, streamProducer: IStreamProducer) =
    
    let checkEvent = EventClusterClient.checkEvent clusterClient

    let publish (eventId: string) (event: DomainEvent) = streamProducer.Produce(eventId, event)
    
    let handle command =
        match command with
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
        | _ -> raise(ArgumentException("unknown command"))
            
    member _.Handle command = handle command 
