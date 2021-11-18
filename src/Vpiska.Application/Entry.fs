namespace Vpiska.Application

open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open Vpiska.Application.Event.CommandHandler
open Vpiska.Application.User.CommandHandler

[<Extension>]
type Entry() =
    
    [<Extension>]
    static member AddUserPersistence(services: IServiceCollection) =
        services.AddSingleton<UserPersistence>() |> ignore
    
    [<Extension>]
    static member AddEventPersistence(services: IServiceCollection, areas: string[]) =
        services.AddSingleton<OrleansPubSub>() |> ignore
        let areaSettings = { Areas = areas }
        services.AddSingleton(areaSettings) |> ignore
        services.AddSingleton<EventPersistence>() |> ignore
