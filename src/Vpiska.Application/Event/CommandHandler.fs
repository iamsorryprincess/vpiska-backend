module Vpiska.Application.Event.CommandHandler

open Orleans
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Interfaces

type AreaSettings =
    { Areas: string[] }

type EventPersistence =
    { ClusterClient: IClusterClient
      StreamProducer: IStreamProducer
      AreaSettings: AreaSettings }
    
let private checkArea (areas: string[]) (area: string) = areas |> Array.contains area

let private publish (producer: IStreamProducer) (eventId: string) (event: DomainEvent) = producer.Produce(eventId, event)

let private createEvent (persistence: EventPersistence) =
    let checkArea = checkArea persistence.AreaSettings.Areas
    let checkOwner = EventClusterClient.checkOwner persistence.ClusterClient
    let createEvent = EventClusterClient.createEvent persistence.ClusterClient
    Domain.createEvent checkArea checkOwner createEvent
    
let private connectUserToChat (persistence: EventPersistence) =
    let checkEvent = EventClusterClient.checkEvent persistence.ClusterClient
    let getUsers = EventClusterClient.getUsers persistence.ClusterClient
    let addUser = EventClusterClient.addUser persistence.ClusterClient
    let publish = publish persistence.StreamProducer
    Domain.connectUserToChat checkEvent getUsers addUser publish
    
let private disconnectUserFromChat (persistence: EventPersistence) =
    let checkEvent = EventClusterClient.checkEvent persistence.ClusterClient
    let removeUser = EventClusterClient.removeUser persistence.ClusterClient
    let publish = publish persistence.StreamProducer
    Domain.disconnectUserFromChat checkEvent removeUser publish
    
let private sendChatMessage (persistence: EventPersistence) =
    let checkEvent = EventClusterClient.checkEvent persistence.ClusterClient
    let addMessage = EventClusterClient.addMessage persistence.ClusterClient
    let publish = publish persistence.StreamProducer
    Domain.sendChatMessage checkEvent addMessage publish
    
let handle persistence command =
    match command with
    | CreateEvent args -> createEvent persistence args
    | Subscribe args -> Domain.subscribe persistence.StreamProducer.TrySubscribe args
    | Unsubscribe args -> Domain.unsubscribe persistence.StreamProducer.TryUnsubscribe args
    | LogUserInChat args -> connectUserToChat persistence args
    | LogoutUserFromChat args -> disconnectUserFromChat persistence args
    | SendChatMessage args -> sendChatMessage persistence args
