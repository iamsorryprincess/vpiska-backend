namespace Vpiska.Application.Event.WebSocket

open System
open System.Collections.Concurrent
open Vpiska.Domain.Event

module ChatStorage =
    
    let private eventGroups = ConcurrentDictionary<string, ConcurrentDictionary<Guid, UserInfo>>()
    let private usersConnections = ConcurrentDictionary<string, Guid>()
    
    let private tryRemove (key: 'a) (dict: ConcurrentDictionary<'a, 'b>) =
        match dict.TryRemove(key) with
        | true, v -> Some v
        | false, _ -> None
        
    let private tryGet (key: 'a) (dict: ConcurrentDictionary<'a, 'b>) =
        match dict.TryGetValue(key) with
        | true, v -> Some v
        | false, _ -> None

    let exist eventId = eventGroups.ContainsKey eventId
    
    let createUserGroup eventId = eventGroups.TryAdd(eventId, ConcurrentDictionary<Guid, UserInfo>())
    
    let removeUserGroup eventId =
        match tryRemove eventId eventGroups with
        | Some _ -> true
        | None -> false
        
    let getUsersCount eventId =
        let mutable users = null
        if eventGroups.TryGetValue(eventId, &users)
        then users.Count else 0
        
    let getUserInfo eventId connectionId =
        match tryGet eventId eventGroups with
        | None -> None
        | Some users -> tryGet connectionId users
    
    let addUserInfo eventId connectionId userInfo =
        match tryGet eventId eventGroups with
        | None -> false
        | Some users -> users.TryAdd(connectionId, userInfo) && usersConnections.TryAdd(userInfo.Id, connectionId)

    let removeUserInfo eventId connectionId =
        match tryGet eventId eventGroups with
        | None -> false
        | Some users ->
            match tryRemove connectionId users with
            | None -> false
            | Some user ->
                match tryRemove user.Id usersConnections with
                | None -> false
                | Some _ -> true
                
    let getUserConnectionId userId = tryGet userId usersConnections
    
    let getUsersConnections eventId =
        match tryGet eventId eventGroups with
        | None -> None
        | Some users -> users |> Seq.map (fun pair -> pair.Key) |> Array.ofSeq |> Some
    
    let getUsersConnectionsExceptOne eventId userId =
        match tryGet eventId eventGroups with
        | None -> None
        | Some users ->
            users
            |> Seq.filter (fun pair -> pair.Value.Id = userId |> not)
            |> Seq.map (fun pair -> pair.Key)
            |> Array.ofSeq
            |> Some
