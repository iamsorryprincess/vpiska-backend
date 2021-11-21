module Vpiska.Application.Event.EventClusterClient

open Orleans
open FSharp.Control.Tasks
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Interfaces

let private getAreaGrain (client: IClusterClient) = client.GetGrain<IAreaGrain>

let private getEventGrain (client: IClusterClient) = client.GetGrain<IEventGrain>

let checkOwner (client: IClusterClient) (ownerId: string) (area: string) =
    let areaGrain = getAreaGrain client area
    areaGrain.CheckOwnerId ownerId
    
let createEvent (client: IClusterClient) (area: string) (event: Event) =
    task {
        let areaGrain = getAreaGrain client area
        let eventGrain = getEventGrain client event.Id
        do! eventGrain.SetData(event, areaGrain)
        let! result = areaGrain.AddEvent eventGrain
        return result
    }
    
let closeEvent (client: IClusterClient) (eventId: string) =
    let eventGrain = getEventGrain client eventId
    eventGrain.Close()
    
let checkOwnership (client: IClusterClient) (eventId: string) (ownerId: string) =
    let eventGrain = getEventGrain client eventId
    eventGrain.CheckOwnership ownerId
    
let checkEvent (client: IClusterClient) (eventId: string) =
    let grain = getEventGrain client eventId
    grain.CheckData()
    
let addUser (client: IClusterClient) (eventId: string) (userInfo: UserInfo) =
    let grain = getEventGrain client eventId
    grain.TryAddUser userInfo
    
let getUsers (client: IClusterClient) (eventId: string) =
    let grain = getEventGrain client eventId
    grain.GetUsers()
    
let removeUser (client: IClusterClient) (eventId: string) (userId: string) =
    let grain = getEventGrain client eventId
    grain.TryRemoveUser userId
    
let addMessage (client: IClusterClient) (eventId: string) (chatData: ChatData) =
    let grain = getEventGrain client eventId
    grain.AddChatData chatData
    
let addMedia (client: IClusterClient) (eventId: string) (mediaLink: string) =
    let grain = getEventGrain client eventId
    grain.TryAddMedia mediaLink
    
let removeMedia (client: IClusterClient) (eventId: string) (mediaLink: string) =
    let grain = getEventGrain client eventId
    grain.TryRemoveMedia mediaLink
