namespace Vpiska.Api.Filters

open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Filters
open Serilog
open Vpiska.Domain.Errors
open Vpiska.Domain.Responses

type ExceptionFilter(logger: ILogger) =
    interface IAsyncExceptionFilter with
        member _.OnExceptionAsync(context: ExceptionContext) =
            logger.Error(context.Exception, "Unknown error")
            context.Result <- ObjectResult(Response.error([|AppError.InternalError|]), StatusCode = 200)
            context.ExceptionHandled <- true
            Task.CompletedTask
