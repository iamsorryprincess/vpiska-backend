namespace Vpiska.Domain.User

type LoginResponse =
    { UserId: string
      AccessToken: string }
    
type Response =
    | UserLogged of LoginResponse
    | Code
    | PasswordChanged
    | UserUpdated
