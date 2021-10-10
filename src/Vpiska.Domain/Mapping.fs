module Vpiska.Domain.Mapping

open System
open Vpiska.Domain.Models
open Vpiska.Domain.Commands

let private phoneCode = "+7"

let mapCreateUserArgs (args: CreateUserArgs): User =
    { Id = Guid.NewGuid().ToString(); Name = args.Name; Phone = args.Phone
      PhoneCode = phoneCode; ImageUrl = null; Password = args.Password; VerificationCode = 0 }
