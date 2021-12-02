using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Vpiska.Application;
using Vpiska.Domain.User;

namespace Vpiska.Api.Rest.Filters
{
    public sealed class ExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger _logger;
        
        public ExceptionFilter(ILogger logger)
        {
            _logger = logger;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            _logger.Error(context.Exception, "Unknown error");

            context.Result =
                new ObjectResult(new ApiResponse(false, null,
                    new[] { new ErrorResponse(Errors.mapAppError(AppError.InternalError)) })) { StatusCode = 200 };
            
            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }
    }
}