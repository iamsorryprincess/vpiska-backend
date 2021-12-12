module Vpiska.Api.Chat.Infrastructure.Jwt

open System.Text
open Microsoft.IdentityModel.Tokens

let internal key = "vpiska_secretkey!123"
let internal issuer = "VpiskaServer"
let internal audience = "VpiskaClient"

let internal getKey (key: string) = key |> Encoding.ASCII.GetBytes |> SymmetricSecurityKey
