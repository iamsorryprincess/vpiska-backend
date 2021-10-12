namespace Vpiska.Api

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.OpenApi.Models
open Vpiska.Api.Filters
open Vpiska.Serilog
open Vpiska.JwtAuth
open Vpiska.Mongo
open Vpiska.Firebase

type Startup(configuration: IConfiguration) =

    member _.ConfigureServices(services: IServiceCollection) =
        services.AddSwaggerGen(fun options -> options.SwaggerDoc("v1", OpenApiInfo(Version = "v1", Title = "api"))) |> ignore
        services.AddControllers(fun options -> options.Filters.Add<ExceptionFilter>() |> ignore) |> ignore
        services.AddSerilog()
        services.AddJwt()
        services.AddMongo(configuration.GetSection("Mongo"))
        services.AddFirebase()

    member _.Configure(app: IApplicationBuilder) =
        app.UseRouting() |> ignore
        app.UseSwagger() |> ignore
        app.UseSwaggerUI(fun c -> c.SwaggerEndpoint("/swagger/v1/swagger.json", "api")) |> ignore
        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore
        app.UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore) |> ignore