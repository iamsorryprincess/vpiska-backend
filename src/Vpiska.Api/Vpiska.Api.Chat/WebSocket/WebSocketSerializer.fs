module Vpiska.Api.Chat.WebSocket.WebSocketSerializer

open System.Text
open System.Text.Json

let private options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

let private getString (data: byte[]) = Encoding.UTF8.GetString(data)

let serialize<'a> (data: 'a) =
    let json = JsonSerializer.Serialize(data, options)
    Encoding.UTF8.GetBytes json
    
let deserialize<'a> (data: byte[]) =
    let json = getString data
    JsonSerializer.Deserialize<'a>(json, options)
    
let deserializeToString data = getString data
