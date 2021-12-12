namespace Vpiska.Api.Chat.Consumers

open System.Threading.Tasks
open Serilog
open Vpiska.Api.Chat.Connectors
open Vpiska.Api.Chat.Infrastructure
open Vpiska.Api.Chat.WebSocket
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Orleans.Interfaces
open Vpiska.Infrastructure.Websocket

type ChatConsumer(logger: ILogger, webSocket: IWebSocketInteracting<ChatConnector>) =
    
    let logError = logger.Error
    
    let send connectionId data = webSocket.SendMessage(connectionId, data)
    
    let closeEvent = EventsLogic.handleEventClosed Storage.getUsersConnections
                                                   Storage.removeUserInfo
                                                   Storage.removeUserGroup
                                                   WebSocketSerializer.serialize
                                                   send
                                                   webSocket.Close
                                                   logError
    
    let handle eventId event =
        match event with
        | UserLoggedIn args ->
            EventsLogic.handleUserLoggedIn Storage.getUserConnectionId Storage.getUsersConnectionsExceptOne WebSocketSerializer.serialize send eventId args
        | UserLoggedOut args ->
            EventsLogic.handleUserLoggedOut Storage.getUsersConnections WebSocketSerializer.serialize send eventId args
        | ChatMessageSent args ->
            EventsLogic.handleChatMessage Storage.getUsersConnections WebSocketSerializer.serialize send eventId args
        | EventClosed -> closeEvent eventId
        | _ -> Task.CompletedTask

    interface IStreamConsumer with
        member this.Consume(eventId, domainEvent) =
            handle eventId domainEvent
