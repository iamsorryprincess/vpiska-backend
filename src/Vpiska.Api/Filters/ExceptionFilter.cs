using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Vpiska.Domain.Base;
using Vpiska.Domain.Constants;

namespace Vpiska.Api.Filters
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
                new ObjectResult(DomainResponse.CreateError(new[]
                    { ErrorResponse.Create(AppErrorConstants.InternalError) })) { StatusCode = 200 };
            
            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }
    }
}