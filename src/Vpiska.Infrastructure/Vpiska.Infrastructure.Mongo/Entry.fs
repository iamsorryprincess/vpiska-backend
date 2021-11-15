namespace Vpiska.Infrastructure.Mongo

open System.Runtime.CompilerServices
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Conventions
open MongoDB.Driver
open Vpiska.Domain.User

type MongoSettings =
    { ConnectionString: string
      DatabaseName: string }

[<Extension>]
type Entry() =
    
    [<Extension>]
    static member AddMongo(services: IServiceCollection, mongoSection: IConfigurationSection) =
        let conventionPack = ConventionPack()
        CamelCaseElementNameConvention() |> conventionPack.Add
        ImmutableTypeClassMapConvention() |> conventionPack.Add
        ConventionRegistry.Register("default", conventionPack, fun t -> true)
        BsonClassMap.RegisterClassMap<User>(fun cm -> cm.AutoMap(); cm.MapIdMember(fun c -> c.Id) |> ignore) |> ignore
        let settings = { ConnectionString = mongoSection.["ConnectionString"]
                         DatabaseName = mongoSection.["DatabaseName"] }
        let client = MongoClient settings.ConnectionString
        services.AddSingleton(settings) |> ignore
        services.AddSingleton(client) |> ignore
    