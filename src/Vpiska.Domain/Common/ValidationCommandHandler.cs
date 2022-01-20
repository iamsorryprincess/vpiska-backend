using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Domain.Common
{
    internal abstract class ValidationCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {
        private readonly IValidator<TCommand> _validator;

        protected ValidationCommandHandler(IValidator<TCommand> validator)
        {
            _validator = validator;
        }

        public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            var result = await Handle(command, cancellationToken);
            return result;
        }

        protected abstract Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }
    
    internal abstract class ValidationCommandHandler<TCommand> : ICommandHandler<TCommand>
    {
        private readonly IValidator<TCommand> _validator;

        protected ValidationCommandHandler(IValidator<TCommand> validator)
        {
            _validator = validator;
        }
        
        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateRequest(command, cancellationToken: cancellationToken);
            await Handle(command, cancellationToken);
        }

        protected abstract Task Handle(TCommand command, CancellationToken cancellationToken);
    }
}