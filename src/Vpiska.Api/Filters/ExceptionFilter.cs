using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Vpiska.Api.Responses;
using Vpiska.Domain.Common.Exceptions;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.Api.Filters
{
    public sealed class ExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ValidationException ex:
                    context.Result = new ObjectResult(ApiResponse.Error(ex.ErrorsCodes)) { StatusCode = 200 };
                    break;
                case DomainException ex:
                    context.Result = new ObjectResult(ApiResponse.Error(ex.ErrorsCodes)) { StatusCode = 200 };
                    break;
                default:
                    _logger.LogError(context.Exception, "Unknown error while handling http request");
                    context.Result = new ObjectResult(ApiResponse.Error("InternalError")) { StatusCode = 200 };
                    break;
            }
            
            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }
    }
}