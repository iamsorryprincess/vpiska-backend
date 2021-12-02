namespace Vpiska.Domain.User

type UserId = string
type Username = string
type PhoneCode = string
type Phone = string
type ImageId = string
type HashedPassword = string
type VerificationCode = int

type User =
    { Id: UserId
      Name: Username
      PhoneCode: PhoneCode
      Phone: Phone
      ImageId: ImageId
      Password: HashedPassword
      VerificationCode: VerificationCode }
    
type CheckPhoneNameResult =
    { IsPhoneExist: bool
      IsNameExist: bool }
