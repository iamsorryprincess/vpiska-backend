namespace Vpiska.Api.Control

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Vpiska.Infrastructure.Orleans

module Program =
    
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore)

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().AddClusterClientShutdown().Run()
        0