namespace Vpiska.Application.Event

open Orleans
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Commands
open Vpiska.Domain.Event.Logic
open Vpiska.Infrastructure.Orleans.Interfaces

type ChatCommandHandler(clusterClient: IClusterClient, streamProducer: IStreamProducer) =
    
    let checkEvent = EventsCluster.checkEvent clusterClient

    let publish (eventId: string) (event: DomainEvent) = streamProducer.Produce(eventId, event)
    
    let handle command =
        match command with
        | Subscribe args -> CommandsLogic.subscribe streamProducer.TrySubscribe args
        | Unsubscribe args -> CommandsLogic.unsubscribe streamProducer.TryUnsubscribe args
        | LogUserInChat args ->
            let getUsers = EventsCluster.getUsers clusterClient
            let addUser = EventsCluster.addUser clusterClient
            CommandsLogic.connectUserToChat checkEvent getUsers addUser publish args
        | LogoutUserFromChat args ->
            let removeUser = EventsCluster.removeUser clusterClient
            CommandsLogic.disconnectUserFromChat checkEvent removeUser publish args
        | SendChatMessage args ->
            let addMessage = EventsCluster.addMessage clusterClient
            CommandsLogic.sendChatMessage checkEvent addMessage publish args
            
    member _.Handle command = handle command 
