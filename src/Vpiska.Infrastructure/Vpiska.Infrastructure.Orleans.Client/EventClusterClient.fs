module Vpiska.Infrastructure.Orleans.Client.EventClusterClient

open Orleans
open FSharp.Control.Tasks
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Grains.Interfaces

let checkOwner (client: IClusterClient) (ownerId: string) (area: string) =
    let areaGrain = client.GetGrain<IAreaGrain> area
    areaGrain.CheckOwnerId ownerId
    
let createEvent (client: IClusterClient) (area: string) (event: Event) =
    task {
        let areaGrain = client.GetGrain<IAreaGrain> area
        let eventGrain = client.GetGrain<IEventGrain> event.Id
        do! eventGrain.SetData(event, areaGrain)
        let! result = areaGrain.AddEvent eventGrain
        return result
    }
    
let createSubscription
    (userLoggedInPubSub: IOrleansPubSubProvider<UserLoggedInArgs>)
    (userLoggedOutPubSub: IOrleansPubSubProvider<string>)
    (chatPubSub: IOrleansPubSubProvider<ChatData>)
    (eventId: string) =
    task {
        let! isLoggedInSubscribed = userLoggedInPubSub.TrySubscribe eventId
        let! isLoggedOutSubscribed = userLoggedOutPubSub.TrySubscribe eventId
        let! isChatSubscribed = chatPubSub.TrySubscribe eventId
        return isLoggedInSubscribed && isLoggedOutSubscribed && isChatSubscribed
    }
    
let removeSubscription
    (userLoggedInPubSub: IOrleansPubSubProvider<UserLoggedInArgs>)
    (userLoggedOutPubSub: IOrleansPubSubProvider<string>)
    (chatPubSub: IOrleansPubSubProvider<ChatData>)
    (eventId: string) =
    task {
        let! isLoggedInUnsubscribed = userLoggedInPubSub.TryUnsubscribe eventId
        let! isLoggedOutUnsubscribed = userLoggedOutPubSub.TryUnsubscribe eventId
        let! isChatUnsubscribed = chatPubSub.TryUnsubscribe eventId
        return isLoggedInUnsubscribed && isLoggedOutUnsubscribed && isChatUnsubscribed
    }
