namespace Vpiska.Api.Control.QueryHandlers

open Orleans
open Vpiska.Api.Control.Infrastructure.Event
open Vpiska.Domain.Event
open Vpiska.Domain.Event.Logic

type EventQueriesPersistence =
    { ClusterClient: IClusterClient
      AreaSettings: AreaSettings }

module EventQueryHandler =
    
    let handle persistence query =
        match query with
        | GetEvent args ->
            let getEvent = Cluster.getEvent persistence.ClusterClient
            QueriesLogic.getById getEvent args
        | GetEvents args ->
            let checkArea area = persistence.AreaSettings.Areas |> Array.contains area
            let getEvents = Cluster.getEvents persistence.ClusterClient
            QueriesLogic.getEvents checkArea getEvents args
