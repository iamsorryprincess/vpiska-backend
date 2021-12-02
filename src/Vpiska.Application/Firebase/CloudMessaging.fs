module Vpiska.Application.Firebase.CloudMessaging

open System.Collections.Generic
open System.Threading.Tasks
open FSharp.Control.Tasks
open FirebaseAdmin.Messaging

let pushNotification (send: Message -> Task<string>) (code: int) (token: string) =
    task {
        let data = Dictionary<string, string>()
        data.Add("code", code.ToString())
        data.Add("body", "Введите код для входа")
        data.Add("title", "Код подтверждения")
        let message = Message(Data = data, Token = token)
        let! _ = send message
        ()
    }
