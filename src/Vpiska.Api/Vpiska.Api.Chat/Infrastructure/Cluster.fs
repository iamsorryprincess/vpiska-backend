module Vpiska.Api.Chat.Infrastructure.Cluster

open Orleans
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Interfaces

let private getAreaGrain (client: IClusterClient) = client.GetGrain<IAreaGrain>

let private getEventGrain (client: IClusterClient) = client.GetGrain<IEventGrain>

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
