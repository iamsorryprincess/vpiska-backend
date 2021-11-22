namespace Vpiska.Application.Event

open Orleans
open Vpiska.Domain.Event

type QueryHandler(clusterClient: IClusterClient, areaSettings: EventClusterClient.AreaSettings) =
    
    let handle query =
        match query with
        | GetEvent args ->
            let getEvent = EventClusterClient.getEvent clusterClient
            Domain.getById getEvent args
        | GetEvents args ->
            let checkArea (area: string) = areaSettings.Areas |> Array.contains area
            let getEvents = EventClusterClient.getEvents clusterClient
            Domain.getEvents checkArea getEvents args
    
    member _.Handle query = handle query
