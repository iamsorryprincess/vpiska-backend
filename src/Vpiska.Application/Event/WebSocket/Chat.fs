namespace Vpiska.Application.Event.WebSocket

open System.Threading.Tasks
open Serilog
open Vpiska.Application.Event
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Interfaces
open Vpiska.Infrastructure.Websocket

type ChatConnector(logger: ILogger, commandHandler: ChatCommandHandler) =
    
    let logErrors = ChatWebSocket.logErrors logger
    
    interface IWebSocketConnector with
        member _.OnConnect(connectionId, identityParams, queryParams) =
            let userInfo = { Id = identityParams.[ChatWebSocket.paramUserId]
                             Name = identityParams.[ChatWebSocket.paramUsername]
                             ImageId = identityParams.[ChatWebSocket.paramUserImage] }            
            ConnectionsLogic.handleConnection ChatStorage.createUserGroup ChatStorage.addUserInfo logger.Error logErrors
                                              ChatStorage.exist commandHandler.Handle
                                              connectionId queryParams.[ChatWebSocket.paramEventId] userInfo :> Task
        
        member _.OnDisconnect(connectionId, identityParams, queryParams) =
            ConnectionsLogic.handleDisconnection ChatStorage.removeUserGroup ChatStorage.removeUserInfo
                                                 ChatStorage.getUsersCount logger.Error logErrors
                                                 commandHandler.Handle connectionId
                                                 queryParams.[ChatWebSocket.paramEventId] identityParams.[ChatWebSocket.paramUserId] :> Task
                                                 
type ChatReceiver(logger: ILogger, commandHandler: ChatCommandHandler) =
    
    let logErrors = ChatWebSocket.logErrors logger
    
    interface IWebSocketReceiver with
        member this.Receive(connectionId, data, identityParams, queryParams) =
            ConnectionsLogic.receiveMessage ChatStorage.getUserInfo WebSocketSerializer.deserializeToString
                                            commandHandler.Handle logErrors connectionId queryParams.[ChatWebSocket.paramEventId]
                                            identityParams.[ChatWebSocket.paramUserId] data :> Task
                                            
type ChatConsumer(logger: ILogger, webSocket: IWebSocketInteracting<ChatConnector>) =
    
    let send connectionId data = webSocket.SendMessage(connectionId, data)
    
    let handle eventId event =
        match event with
        | UserLoggedIn args ->
            EventsLogic.handleUserLoggedIn ChatStorage.getUserConnectionId ChatStorage.getUsersConnectionsExceptOne
                                           WebSocketSerializer.serialize send eventId args
        | UserLoggedOut args ->
            EventsLogic.handleUserLoggedOut ChatStorage.getUsersConnections WebSocketSerializer.serialize send eventId args
        | ChatMessageSent args ->
            EventsLogic.handleChatMessage ChatStorage.getUsersConnections WebSocketSerializer.serialize send eventId args
        | EventClosed ->
            EventsLogic.handleEventClosed ChatStorage.getUsersConnections ChatStorage.removeUserInfo ChatStorage.removeUserGroup
                                          WebSocketSerializer.serialize send webSocket.Close logger.Error eventId
        | _ -> () |> Task.FromResult
    
    interface IStreamConsumer with
        member this.Consume(eventId, domainEvent) =
            handle eventId domainEvent :> Task
