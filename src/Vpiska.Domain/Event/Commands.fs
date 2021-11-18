namespace Vpiska.Domain.Event

[<CLIMutable>]
type CreateEventArgs =
    { OwnerId: UserId
      Name: EventName
      Coordinates: string
      Address: string
      Area: string }

[<CLIMutable>]
type CloseEventArgs =
    { OwnerId: UserId
      EventId: EventId }

[<CLIMutable>]
type SubscribeArgs = { EventId: EventId }

[<CLIMutable>]
type AddMediaArgs =
    { EventId: EventId
      OwnerId: UserId
      MediaData: byte[]
      ContentType: string }

[<CLIMutable>]
type RemoveMediaArgs =
    { EventId: EventId
      OwnerId: UserId
      MediaLink: string }

[<CLIMutable>]
type LoginUserArgs =
    { UserId: UserId
      EventId: EventId
      Name: string
      ImageId: string }

[<CLIMutable>]    
type LogoutUserArgs =
    { UserId: UserId
      EventId: EventId }

[<CLIMutable>]    
type UserMessageArgs =
    { UserId: UserId
      EventId: EventId
      Message: string }

type Command =
    | CreateEvent of CreateEventArgs
    | CloseEvent of CloseEventArgs
    | Subscribe of SubscribeArgs
    | Unsubscribe of SubscribeArgs
    | AddMedia of AddMediaArgs
    | RemoveMedia of RemoveMediaArgs
    | LoginUser of LoginUserArgs
    | LogoutUser of LogoutUserArgs
    | UserMessage of UserMessageArgs
