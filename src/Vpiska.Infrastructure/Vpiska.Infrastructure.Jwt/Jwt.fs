module Vpiska.Infrastructure.Jwt.Jwt

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.IdentityModel.Tokens
open Vpiska.Domain.User

let private lifetimeDays = 3.0

let private getClaims (userId: UserId) (name: string) (imageId: string) =
    let claims = ResizeArray<Claim>(2)
    Claim("Id", userId) |> claims.Add
    Claim("Name", name) |> claims.Add
    if String.IsNullOrWhiteSpace imageId |> not then
        Claim("ImageId", imageId) |> claims.Add
    claims
        
let mutable internal key = "vpiska_secretkey!123"
let mutable internal issuer = "VpiskaServer"
let mutable internal audience = "VpiskaClient"

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
