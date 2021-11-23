namespace Vpiska.Domain.Event

type UserLoggedInArgs =
    { User: UserInfo
      OtherUsers: UserInfo[] }
    
type UserLoggedOutArgs = { UserId: UserId }

type MediaArgs = { ImageId: string }

type DomainEvent =
    | EventCreated of Event
    | EventClosed
    | SubscriptionCreated
    | SubscriptionRemoved
    | UserLoggedIn of UserLoggedInArgs
    | UserLoggedOut of UserLoggedOutArgs
    | ChatMessageSent of ChatData
    | MediaAdded of MediaArgs
    | MediaRemoved of MediaArgs
