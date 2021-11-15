namespace Vpiska.Infrastructure.Firebase

open System.Runtime.CompilerServices
open FirebaseAdmin
open Google.Apis.Auth.OAuth2
open Google.Cloud.Storage.V1
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

type FirebaseSettings = { BucketName: string }

[<Extension>]
type Entry() =
    
    [<Extension>]
    static member AddFirebase(services: IServiceCollection, firebaseSection: IConfigurationSection) =
        #if DEBUG
        let path = "../Vpiska.Infrastructure/Vpiska.Infrastructure.Firebase/settings.json"
        #else
        let path = "settings.json"
        #endif
        let settings = { BucketName = firebaseSection.["BucketName"] }
        let firebaseApp = FirebaseApp.Create(AppOptions(Credential = GoogleCredential.FromFile(path)))
        let storageClient = StorageClient.Create(GoogleCredential.FromFile(path))
        services.AddSingleton(settings) |> ignore
        services.AddSingleton(firebaseApp) |> ignore
        services.AddSingleton(storageClient) |> ignore
    