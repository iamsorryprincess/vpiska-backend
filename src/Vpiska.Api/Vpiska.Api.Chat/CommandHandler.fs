namespace Vpiska.Api.Chat

open Orleans
open Vpiska.Api.Chat.Infrastructure
open Vpiska.Infrastructure.Orleans.Interfaces
open Vpiska.Domain.Event.Commands
open Vpiska.Domain.Event.Logic

type Persistence =
    { ClusterClient: IClusterClient
      StreamProducer: IStreamProducer }

module CommandHandler =
    
    let handle persistence command =
        match command with
        | Subscribe args -> CommandsLogic.subscribe persistence.StreamProducer.TrySubscribe args
        | Unsubscribe args -> CommandsLogic.unsubscribe persistence.StreamProducer.TryUnsubscribe args
        | LogUserInChat args ->
            let checkEvent = Cluster.checkEvent persistence.ClusterClient
            let getUsers = Cluster.getUsers persistence.ClusterClient
            let addUser = Cluster.addUser persistence.ClusterClient
            let publish eventId event = persistence.StreamProducer.Produce(eventId, event)
            CommandsLogic.connectUserToChat checkEvent getUsers addUser publish args
        | LogoutUserFromChat args ->
            let checkEvent = Cluster.checkEvent persistence.ClusterClient
            let removeUser = Cluster.removeUser persistence.ClusterClient
            let publish eventId event = persistence.StreamProducer.Produce(eventId, event)
            CommandsLogic.disconnectUserFromChat checkEvent removeUser publish args
        | SendChatMessage args ->
            let checkEvent = Cluster.checkEvent persistence.ClusterClient
            let addMessage = Cluster.addMessage persistence.ClusterClient
            let publish eventId event = persistence.StreamProducer.Produce(eventId, event)
            CommandsLogic.sendChatMessage checkEvent addMessage publish args
