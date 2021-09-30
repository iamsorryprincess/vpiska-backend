using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Vpiska.Api.Dto;

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
            _logger.LogError(context.Exception, "Unknown error");
            context.Result = new ObjectResult(ErrorResponse.Create(context.Exception.Message)) { StatusCode = 500 };
            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }
    }
}