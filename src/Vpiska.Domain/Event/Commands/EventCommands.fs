namespace Vpiska.Domain.Event.Commands

open Vpiska.Domain.Event

[<CLIMutable>]
type CreateEventArgs =
    { EventId: EventId
      OwnerId: UserId
      Name: EventName
      Coordinates: string
      Address: string
      Area: string }

[<CLIMutable>]
type CloseEventArgs =
    { OwnerId: UserId
      EventId: EventId }

[<CLIMutable>]
type AddMediaArgs =
    { EventId: EventId
      OwnerId: UserId
      MediaData: byte[]
      ContentType: string
      ImageId: string }

[<CLIMutable>]
type RemoveMediaArgs =
    { EventId: EventId
      OwnerId: UserId
      MediaLink: string }

type EventCommand =
    | CreateEvent of CreateEventArgs
    | CloseEvent of CloseEventArgs
    | AddMedia of AddMediaArgs
    | RemoveMedia of RemoveMediaArgs
