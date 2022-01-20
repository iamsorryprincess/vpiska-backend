using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User.Exceptions;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Domain.User.Responses;

namespace Vpiska.Domain.User.Queries.CheckCodeQuery
{
    internal sealed class CheckCodeHandler : ValidationQueryHandler<CheckCodeQuery, LoginResponse>
    {
        private readonly IUserRepository _repository;
        private readonly IIdentityService _identityService;

        public CheckCodeHandler(IValidator<CheckCodeQuery> validator,
            IUserRepository repository,
            IIdentityService identityService) : base(validator)
        {
            _repository = repository;
            _identityService = identityService;
        }

        protected override async Task<LoginResponse> Handle(CheckCodeQuery query, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByFieldAsync("phone", query.Phone, cancellationToken);

            if (user == null)
            {
                throw new UserNotFoundException(Constants.UserByPhoneNotFound);
            }

            if (user.VerificationCode != query.Code)
            {
                throw new InvalidCodeException();
            }

            return new LoginResponse()
            {
                UserId = user.Id,
                UserName = user.Name,
                ImageId = user.ImageId,
                AccessToken = _identityService.GetAccessToken(user)
            };
        }
    }
}