namespace Vpiska.Domain.Models

type User =
    { Id: string
      Name: string
      PhoneCode: string
      Phone: string
      ImageUrl: string
      Password: string
      VerificationCode: int }
    
type CheckPhoneNameResult =
    { IsPhoneExist: bool
      IsNameExist: bool }
