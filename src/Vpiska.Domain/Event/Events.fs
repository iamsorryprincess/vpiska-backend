namespace Vpiska.Domain.Event

type UserLoggedInArgs =
    { User: UserInfo
      OtherUsers: UserInfo[] }

type DomainEvent =
    | UserLoggedIn of UserLoggedInArgs
    | UserLoggedOut of UserId
    | ChatMessage of ChatData
