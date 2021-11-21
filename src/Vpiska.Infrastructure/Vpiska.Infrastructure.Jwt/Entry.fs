namespace Vpiska.Infrastructure.Jwt

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens

[<Extension>]
type Entry() =
    
    static let createTokenParams () =
        TokenValidationParameters(
          ValidateIssuer = true,
          ValidIssuer = Jwt.issuer,
          ValidateAudience = true,
          ValidAudience = Jwt.audience,
          ValidateLifetime = true,
          IssuerSigningKey = Jwt.getKey Jwt.key,
          ValidateIssuerSigningKey = true)
        
    static let handle (context: MessageReceivedContext) (tokenQueryParam: string) (webSocketUrl: string) =
        match context.Request.Query.Count with
        | 0 -> Task.CompletedTask
        | _ ->
            let accessToken = context.Request.Query.[tokenQueryParam].Item 0
            let path = context.HttpContext.Request.Path
            if String.IsNullOrWhiteSpace(accessToken) |> not && path.StartsWithSegments(PathString(webSocketUrl)) then
                context.Token <- accessToken
            Task.CompletedTask
    
    [<Extension>]
    static member AddJwt(services: IServiceCollection) =
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options -> options.RequireHttpsMetadata <- false
                                         options.TokenValidationParameters <- createTokenParams()) |> ignore
    
    [<Extension>]   
    static member AddJwtForWebsocket(services: IServiceCollection, tokenQueryParam: string, webSocketUrl: string) =
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options -> options.RequireHttpsMetadata <- false
                                         options.TokenValidationParameters <- createTokenParams()
                                         options.Events <- JwtBearerEvents(OnMessageReceived =
                                             Func<MessageReceivedContext, Task>(fun context -> handle context tokenQueryParam webSocketUrl))) |> ignore
        services.AddAuthorization() |> ignore
    