module Vpiska.Application.Event.CommandHandler

open Orleans
open Vpiska.Domain.Event
open Vpiska.Domain.User
open Vpiska.Infrastructure.Orleans.Client
open Vpiska.Infrastructure.Orleans.Grains.Interfaces

type AreaSettings =
    { Areas: string[] }

type OrleansPubSub =
    { UserLoggedInPubSub: IOrleansPubSubProvider<UserLoggedInArgs>
      UserLoggedOutPubSub: IOrleansPubSubProvider<string>
      ChatPubSub: IOrleansPubSubProvider<ChatData> }

type EventPersistence =
    { ClusterClient: IClusterClient
      PubSubProviders: OrleansPubSub
      AreaSettings: AreaSettings }
    
let private checkArea (areas: string[]) (area: string) = areas |> Array.contains area

let private createEvent (persistence: EventPersistence) =
    let checkArea = checkArea persistence.AreaSettings.Areas
    let checkOwner = EventClusterClient.checkOwner persistence.ClusterClient
    let createEvent = EventClusterClient.createEvent persistence.ClusterClient
    Domain.createEvent checkArea checkOwner createEvent
    
let private subscribe (persistence: EventPersistence) =
    let createSubscription = EventClusterClient.createSubscription
                              persistence.PubSubProviders.UserLoggedInPubSub
                              persistence.PubSubProviders.UserLoggedOutPubSub
                              persistence.PubSubProviders.ChatPubSub
    Domain.subscribe createSubscription
    
let private unsubscribe (persistence: EventPersistence) =
    let removeSubscription = EventClusterClient.removeSubscription
                               persistence.PubSubProviders.UserLoggedInPubSub
                               persistence.PubSubProviders.UserLoggedOutPubSub
                               persistence.PubSubProviders.ChatPubSub
    Domain.unsubscribe removeSubscription
    
let handle persistence command =
    match command with
    | CreateEvent args -> createEvent persistence args
    | Subscribe args -> subscribe persistence args
    | Unsubscribe args -> unsubscribe persistence args
