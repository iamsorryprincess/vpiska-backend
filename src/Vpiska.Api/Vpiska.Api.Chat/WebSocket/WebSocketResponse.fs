namespace Vpiska.Api.Chat.WebSocket

type MessageType =
    | UserLoggedIn = 0
    | UserLoggedOut = 1
    | UsersInfo = 2
    | ChatMessage = 3
    | EventClosed = 4

[<CLIMutable>]
type WebSocketResponse =
    { Type: MessageType
      Data: obj }
