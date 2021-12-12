namespace Vpiska.Api.Control.Infrastructure.Firebase

open System.IO
open System.Net
open FSharp.Control.Tasks
open Google
open Google.Cloud.Storage.V1

type FirebaseSettings = { BucketName: string }

module FileStorage =
    
    let uploadFile (storageClient: StorageClient) (bucketName: string) (imageId: string) (data: byte[]) (contentType: string) =
        task {
            use stream = new MemoryStream(data)
            let! result = storageClient.UploadObjectAsync(bucketName, imageId, contentType, stream)
            return result.Name
        }
        
    let deleteFile (storageClient: StorageClient) (bucketName: string) (imageId: string) =
        task {
            try
                do! storageClient.DeleteObjectAsync(bucketName, imageId)
                return true
            with
            | :? GoogleApiException as ex ->
                match ex.HttpStatusCode with
                | HttpStatusCode.NotFound -> return false
                | _ -> return raise ex
        }
