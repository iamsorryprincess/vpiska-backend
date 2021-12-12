namespace Vpiska.Api.Control.Infrastructure.User

open System
open MongoDB.Driver
open FSharp.Control.Tasks
open Vpiska.Domain.User

type MongoSettings =
    { ConnectionString: string
      DatabaseName: string }

module Database =
    
    let private isNotEmpty input = input |> String.IsNullOrWhiteSpace |> not
    
    let private getCollection (client: MongoClient) databaseName =
        let db = client.GetDatabase databaseName
        db.GetCollection<User> "users"
        
    let private createPhoneFilter (phone: string) = Builders<User>.Filter.Eq((fun x -> x.Phone), phone)
    
    let private createNameFilter (name: string) = Builders<User>.Filter.Eq((fun x -> x.Name), name)
    
    let private createIdFilter (id: string) = Builders<User>.Filter.Eq((fun x -> x.Id), id)
    
    let getUserByPhoneAndName client databaseName (phone: string) (name: string) =
        task {
            let phoneFilter = createPhoneFilter phone
            let nameFilter = createNameFilter name
            let filter = Builders<User>.Filter.Or(phoneFilter, nameFilter)
            let collection = getCollection client databaseName
            
            let! result = collection.Find(filter).Project(fun user ->
                { IsPhoneExist = user.Phone = phone
                  IsNameExist = user.Name = name }).ToListAsync()
            
            match result.Count with
            | 0 -> return { IsPhoneExist = false; IsNameExist = false }
            | _ -> return result |> Seq.fold (fun acc item ->
                match acc with
                | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = false } ->
                    { IsPhoneExist = acc.IsPhoneExist; IsNameExist = item.IsNameExist }
                | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = true } ->
                    { IsPhoneExist = item.IsPhoneExist; IsNameExist = acc.IsNameExist }
                | { CheckPhoneNameResult.IsPhoneExist = true; CheckPhoneNameResult.IsNameExist = true } ->
                    { IsPhoneExist = acc.IsPhoneExist; IsNameExist = acc.IsNameExist }
                | { CheckPhoneNameResult.IsPhoneExist = false; CheckPhoneNameResult.IsNameExist = false } ->
                    { IsPhoneExist = item.IsPhoneExist; IsNameExist = item.IsNameExist }) { IsPhoneExist = false; IsNameExist = false }
        }
        
    let createUser client databaseName (user: User) =
        task {
            let collection = getCollection client databaseName
            do! collection.InsertOneAsync user
            return user.Id
        }
        
    let getUserByPhone client databaseName (phone: string) =
        task {
            let collection = getCollection client databaseName
            let filter = createPhoneFilter phone
            return! collection.Find(filter).FirstOrDefaultAsync()
        }
        
    let getUserById client databaseName (id: string) =
        task {
            let collection = getCollection client databaseName
            let filter = createIdFilter id
            return! collection.Find(filter).FirstOrDefaultAsync()
        }
        
    let setCode client databaseName (phone: string) (code: int) =
        task {
            let filter = createPhoneFilter phone
            let update = Builders<User>.Update.Set((fun x -> x.VerificationCode), code)
            let collection = getCollection client databaseName
            let! result = collection.UpdateOneAsync(filter, update)
            return result.MatchedCount > 0L
        }
        
    let changePassword client databaseName (id: string) (password: string) =
        task {
            let filter = createIdFilter id
            let update = Builders<User>.Update.Set((fun x -> x.Password), password)
            let collection = getCollection client databaseName
            let! result = collection.UpdateOneAsync(filter, update)
            return result.MatchedCount > 0L
        }
        
    let isPhoneExist client databaseName (phone: string) =
        task {
            let filter = createPhoneFilter phone
            let collection = getCollection client databaseName
            return! collection.Find(filter).AnyAsync()
        }
        
    let isNameExist client databaseName (name: string) =
        task {
            let filter = createNameFilter name
            let collection = getCollection client databaseName
            return! collection.Find(filter).AnyAsync()
        }
        
    let updateUser client databaseName (id: string) (name: string) (phone: string) (imageId: string) =
        task {
            let updates = ResizeArray<UpdateDefinition<User>>()
            if isNotEmpty name then
                Builders<User>.Update.Set((fun x -> x.Name), name) |> updates.Add
            if isNotEmpty phone then
                Builders<User>.Update.Set((fun x -> x.Phone), phone) |> updates.Add
            if isNotEmpty imageId then
                Builders<User>.Update.Set((fun x -> x.ImageId), imageId) |> updates.Add
            if updates.Count = 0 then
                return true
            else
                let filter = createIdFilter id
                let update = Builders<User>.Update.Combine(updates)
                let collection = getCollection client databaseName
                let! result = collection.UpdateOneAsync(filter, update)
                return result.MatchedCount > 0L
        }
