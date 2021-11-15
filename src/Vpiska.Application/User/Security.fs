module internal Vpiska.Application.User.Security

open System
open System.Text

let private convertToHash (password: string) = password |> Encoding.UTF8.GetBytes |> Convert.ToBase64String

let hashPassword = convertToHash

let checkPassword (password: string) (hash: string) = convertToHash password = hash
