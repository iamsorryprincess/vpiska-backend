namespace Vpiska.Application.Event

open Orleans
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Logic

type QueryHandler(clusterClient: IClusterClient, areaSettings: EventsCluster.AreaSettings) =
    
    let handle query =
        match query with
        | GetEvent args ->
            let getEvent = EventsCluster.getEvent clusterClient
            QueriesLogic.getById getEvent args
        | GetEvents args ->
            let checkArea (area: string) = areaSettings.Areas |> Array.contains area
            let getEvents = EventsCluster.getEvents clusterClient
            QueriesLogic.getEvents checkArea getEvents args
    
    member _.Handle query = handle query
