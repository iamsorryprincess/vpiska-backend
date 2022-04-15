using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Domain.User.Responses;

namespace Vpiska.Domain.User.Queries.CheckCodeQuery
{
    internal sealed class CheckCodeHandler : IQueryHandler<CheckCodeQuery, LoginResponse>
    {
        private readonly IValidator<CheckCodeQuery> _validator;
        private readonly IUserRepository _repository;
        private readonly IIdentityService _identityService;

        public CheckCodeHandler(IValidator<CheckCodeQuery> validator,
            IUserRepository repository,
            IIdentityService identityService)
        {
            _validator = validator;
            _repository = repository;
            _identityService = identityService;
        }

        public async Task<LoginResponse> HandleAsync(CheckCodeQuery query, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateRequest(query, cancellationToken: cancellationToken);
            var user = await _repository.GetByFieldAsync("phone", query.Phone, cancellationToken);

            if (user == null)
            {
                throw new UserNotFoundException(Constants.UserByPhoneNotFound);
            }

            if (user.VerificationCode != query.Code)
            {
                throw new InvalidCodeException();
            }

            return new LoginResponse(_identityService.GetAccessToken(user), null, user);
        }
    }
}