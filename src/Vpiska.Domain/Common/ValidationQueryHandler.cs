using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Common
{
    internal abstract class ValidationQueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    {
        private readonly IValidator<TQuery> _validator;

        protected ValidationQueryHandler(IValidator<TQuery> validator)
        {
            _validator = validator;
        }

        public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(query, cancellationToken: cancellationToken);
            var result = await Handle(query, cancellationToken);
            return result;
        }

        protected abstract Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
    }
}