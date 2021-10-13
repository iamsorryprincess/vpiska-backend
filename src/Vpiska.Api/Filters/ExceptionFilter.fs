namespace Vpiska.Api.Filters

open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Filters
open Serilog
open Vpiska.Api
open Vpiska.Domain.Errors

type ExceptionFilter(logger: ILogger) =
    interface IAsyncExceptionFilter with
        member _.OnExceptionAsync(context: ExceptionContext) =
            logger.Error(context.Exception, "Unknown error")
            context.Result <- ObjectResult(Http.createErrorResult([|AppError.InternalError|]), StatusCode = 200)
            context.ExceptionHandled <- true
            Task.CompletedTask
