namespace Vpiska.Domain.Event

type GetEventArgs =
    { EventId: string }
    
type GetEventsArgs =
    { Area: string }
    
type Query =
    | GetEvent of GetEventArgs
    | GetEvents of GetEventsArgs

type ShortEventResponse =
    { Id: string
      Name: string
      Coordinates: string
      UsersCount: int }

type QueryResponse =
    | Event of Event
    | Events of ShortEventResponse[]
