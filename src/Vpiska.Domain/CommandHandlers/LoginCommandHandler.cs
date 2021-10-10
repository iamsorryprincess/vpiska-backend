using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Requests;
using Vpiska.Domain.Responses;
using Vpiska.Domain.Validation;

namespace Vpiska.Domain.CommandHandlers
{
    public sealed class LoginCommandHandler : ICommandHandler<LoginRequest, LoginResponse>
    {
        private readonly IValidator<LoginRequest> _validator;
        private readonly IUserStorage _storage;
        private readonly IAuthService _auth;

        public LoginCommandHandler(IValidator<LoginRequest> validator,
            IUserStorage storage,
            IAuthService auth)
        {
            _validator = validator;
            _storage = storage;
            _auth = auth;
        }

        public async Task<DomainResponse<LoginResponse>> Handle(LoginRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(error => ErrorResponse.Create(error.ErrorMessage))
                    .ToList();
                return DomainResponse<LoginResponse>.CreateError(errors);
            }

            var user = await _storage.GetUserByPhone(request.Phone);

            if (user == null)
            {
                var response = DomainResponse<LoginResponse>.CreateError();
                response.Errors.Add(ErrorResponse.Create(ErrorCodes.UserByPhoneNotFound));
                return response;
            }

            if (!_auth.CheckPassword(user.Password, request.Password))
            {
                var response = DomainResponse<LoginResponse>.CreateError();
                response.Errors.Add(ErrorResponse.Create(ErrorCodes.InvalidPassword));
                return response;
            }
            
            var result = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = _auth.GetEncodedJwt(user.Id, user.Name, user.ImageUrl)
            };
            
            return DomainResponse<LoginResponse>.CreateSuccess(result);
        }
    }
}