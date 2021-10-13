namespace Vpiska.Domain.Responses

type LoginResponse =
    { UserId: string
      AccessToken: string }
    
type DomainResponse =
    | Login of LoginResponse
    | Code
    | PasswordChanged
    | UserUpdated
