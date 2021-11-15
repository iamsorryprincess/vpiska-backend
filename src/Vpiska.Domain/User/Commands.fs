namespace Vpiska.Domain.User

open System

[<CLIMutable>]
type CreateUserArgs =
    { Phone: string
      Name: string
      Password: string
      ConfirmPassword: string }
    member self.toDomain (id: UserId) (phoneCode: PhoneCode) (password: string) =
        { Id = id; Name = self.Name; PhoneCode = phoneCode
          Phone = self.Phone; ImageId = null; Password = password; VerificationCode = 0 }

[<CLIMutable>]
type LoginUserArgs =
    { Phone: string
      Password: string }
    
[<CLIMutable>]
type CodeArgs =
    { Phone: string
      Token: string }

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
      ImageData: byte[]
      ContentType: string }
    
type Command =
    | Create of CreateUserArgs
    | Login of LoginUserArgs
    | SetCode of CodeArgs
    | CheckCode of CheckCodeArgs
    | ChangePassword of ChangePasswordArgs
    | Update of UpdateUserArgs
