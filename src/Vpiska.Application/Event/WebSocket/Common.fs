namespace Vpiska.Application.Event.WebSocket

open System.Text
open System.Text.Json

type MessageType =
    | UserLoggedIn = 0
    | UserLoggedOut = 1
    | UsersInfo = 2
    | ChatMessage = 3

[<CLIMutable>]
type WebSocketResponse =
    { Type: MessageType
      Data: obj }
    
module internal WebSocketSerializer =
    
    let private options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
    
    let private getString (data: byte[]) = Encoding.UTF8.GetString(data)
    
    let serialize<'a> (data: 'a) =
        let json = JsonSerializer.Serialize(data, options)
        Encoding.UTF8.GetBytes json
    
    let deserialize<'a> (data: byte[]) =
        let json = getString data
        JsonSerializer.Deserialize<'a>(json, options)
        
    let deserializeToString data = getString data
