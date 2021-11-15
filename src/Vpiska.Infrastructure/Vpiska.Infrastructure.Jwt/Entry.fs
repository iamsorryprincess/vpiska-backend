namespace Vpiska.Infrastructure.Jwt

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens

[<Extension>]
type Entry() =
    
    [<Extension>]
    static member AddJwt(services: IServiceCollection) =
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options -> options.RequireHttpsMetadata <- false
                                         options.TokenValidationParameters <- TokenValidationParameters(
                                             ValidateIssuer = true,
                                             ValidIssuer = Jwt.issuer,
                                             ValidateAudience = true,
                                             ValidAudience = Jwt.audience,
                                             ValidateLifetime = true,
                                             IssuerSigningKey = Jwt.getKey Jwt.key,
                                             ValidateIssuerSigningKey = true)) |> ignore
    