namespace Vpiska.Domain.Event

type UserLoggedInArgs =
    { User: UserInfo
      OtherUsers: UserInfo[] }
    
type UserLoggedOutArgs = { UserId: UserId }

type DomainEvent =
    | UserLoggedIn of UserLoggedInArgs
    | UserLoggedOut of UserLoggedOutArgs
    | ChatMessage of ChatData
