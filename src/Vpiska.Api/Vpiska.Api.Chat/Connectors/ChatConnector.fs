namespace Vpiska.Api.Chat.Connectors

open Serilog
open Vpiska.Api.Chat
open Vpiska.Api.Chat.Infrastructure
open Vpiska.Api.Chat.WebSocket
open Vpiska.Domain.Event
open Vpiska.Infrastructure.Websocket

type ChatConnector(logger: ILogger, persistence: Persistence) =
    
    let eventId = "eventId"
    let userId = "Id"
    let username = "Name"
    let userImage = "ImageId"
    
    let logError = logger.Error
    
    let logErrors (errors: AppError[]) =
        for error in errors do
            error |> Errors.mapAppError |> logger.Error
            
    let handleCommand = CommandHandler.handle persistence
    
    let subscribe = ConnectionsLogic.subscribe Storage.createUserGroup logError logErrors handleCommand
    
    let unsubscribe = ConnectionsLogic.unsubscribe Storage.removeUserGroup logError logErrors handleCommand
    
    let connect = ConnectionsLogic.connectUser Storage.addUserInfo logError logErrors handleCommand
    
    let disconnect = ConnectionsLogic.disconnectUser Storage.removeUserInfo logError logErrors handleCommand
    
    let handleConnection = ConnectionsLogic.handleConnection Storage.exist connect subscribe
    
    let handleDisconnection = ConnectionsLogic.handleDisconnection Storage.getUsersCount disconnect unsubscribe
    
    interface IWebSocketConnector with
        member this.OnConnect(connectionId, identityParams, queryParams) =
            let userInfo = { Id = identityParams.[userId]
                             Name = identityParams.[username]
                             ImageId = identityParams.[userImage] }
            handleConnection connectionId queryParams.[eventId] userInfo
        
        member this.OnDisconnect(connectionId, identityParams, queryParams) =
            handleDisconnection connectionId queryParams.[eventId] identityParams.[userId]
