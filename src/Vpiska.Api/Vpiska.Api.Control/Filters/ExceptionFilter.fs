namespace Vpiska.Api.Control.Filters

open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Filters
open Serilog
open Vpiska.Api.Control.Http
open Vpiska.Domain.User

type ExceptionFilter(logger: ILogger) =
    
    interface IAsyncExceptionFilter with
        member this.OnExceptionAsync(context) =
            logger.Error(context.Exception, "Unknown error")
            context.Result <- ObjectResult({ IsSuccess = false
                                             Result = null
                                             Errors = [| { ErrorCode = AppError.InternalError |> Errors.mapAppError } |] }, StatusCode = 200)
            context.ExceptionHandled <- true
            Task.CompletedTask
