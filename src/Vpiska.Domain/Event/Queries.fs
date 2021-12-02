namespace Vpiska.Domain.Event

type GetEventArgs =
    { EventId: string }
    
type GetEventsArgs =
    { Area: string }
    
type Query =
    | GetEvent of GetEventArgs
    | GetEvents of GetEventsArgs
