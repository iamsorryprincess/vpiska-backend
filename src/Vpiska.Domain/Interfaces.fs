namespace Vpiska.Domain

open System.IO
open System.Threading.Tasks
open Vpiska.Domain.Models

type IUserRepository =
    abstract member GetById: id: string -> Task<User voption>
    abstract member Create: user: User -> Task<string>
    abstract member Update: id: string -> name: string -> phone: string -> imageUrl: string -> Task<bool>
    abstract member CheckInfo: phone: string -> name: string -> Task<CheckPhoneNameResult>
    abstract member GetUserByPhone: phone: string -> Task<User voption>
    abstract member SetVerificationCode: phone: string -> code: int -> Task<bool>
    abstract member ChangePassword: id: string -> password: string -> Task<bool>
    
type IAuth =
    abstract member HashPassword: password: string -> string
    abstract member CheckPassword: hashedPassword: string -> password: string -> bool
    abstract member GetToken: userId: string -> username: string -> imageUrl: string -> string
    
type IFileStorage =
    abstract member UploadFile: fileName: string -> contentType: string -> stream: Stream -> Task<string>
    abstract member DeleteFile: url: string -> Task<bool>
    
type INotificationService =
    abstract member SendVerificationCode: code: int -> Task
