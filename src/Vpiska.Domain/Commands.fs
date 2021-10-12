namespace Vpiska.Domain.Commands

open System

[<CLIMutable>]
type CreateUserArgs =
    { Phone: string
      Name: string
      Password: string
      ConfirmPassword: string }

[<CLIMutable>]
type LoginUserArgs =
    { Phone: string
      Password: string }
    
[<CLIMutable>]
type CodeArgs = { Phone: string }

[<CLIMutable>]
type CheckCodeArgs =
    { Phone: string
      Code: Nullable<int> }

[<CLIMutable>]   
type ChangePasswordArgs =
    { Id: string
      Password: string
      ConfirmPassword: string }

[<CLIMutable>]    
type UpdateUserArgs =
    { Id: string
      Name: string
      Phone: string
      ImageData: byte[] voption
      ContentType: string }
    
type Command =
    | CreateUser of CreateUserArgs
    | LoginUser of LoginUserArgs
    | SetCode of CodeArgs
    | CheckCode of CheckCodeArgs
    | ChangePassword of ChangePasswordArgs
    | UpdateUser of UpdateUserArgs
