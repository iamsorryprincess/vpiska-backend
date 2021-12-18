namespace Vpiska.Domain.Event.Commands

open Vpiska.Domain.Event

[<CLIMutable>]
type SubscribeArgs = { EventId: EventId }

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
type ChatMessageArgs =
    { UserId: UserId
      UserName: string
      UserImage: string
      EventId: EventId
      Message: string }

type ChatCommand =
    | Subscribe of SubscribeArgs
    | Unsubscribe of SubscribeArgs
    | LogUserInChat of LoginUserArgs
    | LogoutUserFromChat of LogoutUserArgs
    | SendChatMessage of ChatMessageArgs
