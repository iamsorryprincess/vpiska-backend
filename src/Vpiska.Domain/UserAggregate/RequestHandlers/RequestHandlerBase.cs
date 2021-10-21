using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Vpiska.Domain.UserAggregate.Responses;

namespace Vpiska.Domain.UserAggregate.RequestHandlers
{
    public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
        
        protected DomainResponse Error(params string[] errorCodes) =>
            DomainResponse.CreateError(errorCodes.Select(ErrorResponse.Create).ToArray());

        protected DomainResponse<T> Error<T>(params string[] errorCodes)
            where T : class =>
            DomainResponse<T>.CreateError(errorCodes.Select(ErrorResponse.Create).ToArray());
        
        protected DomainResponse Success() => DomainResponse.CreateSuccess();

        protected DomainResponse<T> Success<T>(T response) where T : class =>
            DomainResponse<T>.CreateSuccess(response);
    }
}