namespace Vpiska.Domain.User

type UserLoggedArgs =
    { UserId: string
      AccessToken: string }
    
type DomainEvent =
    | UserLogged of UserLoggedArgs
    | CodePushed
    | PasswordChanged
    | UserUpdated
