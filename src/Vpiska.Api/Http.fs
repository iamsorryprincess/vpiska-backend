module Vpiska.Api.Http

open Vpiska.Domain.Errors
open Vpiska.Domain.Responses

let private mapResult<'a> result =
    match result with
    | Login args -> Response<'a>.success(args) :> obj
    | _ -> Response.success () :> obj
    
let mapResponse (response: Result<DomainResponse, AppError[]>) =
    match response with
    | Error errors -> Response.error errors :> obj
    | Ok result -> mapResult result
