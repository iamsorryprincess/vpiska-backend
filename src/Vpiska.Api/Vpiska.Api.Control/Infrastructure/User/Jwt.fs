module Vpiska.Api.Control.Infrastructure.User.Jwt

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.IdentityModel.Tokens
open Vpiska.Domain.User

let private lifetimeDays = 3.0

let private getClaims (userId: UserId) (name: string) (imageId: string) =
    match String.IsNullOrWhiteSpace imageId with
    | true ->
        let claims = ResizeArray<Claim>(2)
        Claim("Id", userId) |> claims.Add
        Claim("Name", name) |> claims.Add
        claims
    | false ->
        let claims = ResizeArray<Claim>(3)
        Claim("Id", userId) |> claims.Add
        Claim("Name", name) |> claims.Add
        Claim("ImageId", imageId) |> claims.Add
        claims

let internal key = "vpiska_secretkey!123"
let internal issuer = "VpiskaServer"
let internal audience = "VpiskaClient"

let internal getKey (key: string) = key |> Encoding.ASCII.GetBytes |> SymmetricSecurityKey

let encodeJwt (userId: UserId) (name: string) (imageId: string) =
    let claims = getClaims userId name imageId
    let now = DateTime.UtcNow
    let identity = ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType)
    let securityKey = getKey key
    let jwt = JwtSecurityToken(issuer = issuer, audience = audience, notBefore = now,
                               claims = identity.Claims, expires = now.Add(TimeSpan.FromDays(lifetimeDays)),
                               signingCredentials = SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256))
    jwt |> JwtSecurityTokenHandler().WriteToken
