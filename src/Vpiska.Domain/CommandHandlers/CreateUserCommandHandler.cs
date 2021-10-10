using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Models;
using Vpiska.Domain.Requests;
using Vpiska.Domain.Responses;
using Vpiska.Domain.Validation;

namespace Vpiska.Domain.CommandHandlers
{
    public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserRequest, LoginResponse>
    {
        private readonly IValidator<CreateUserRequest> _validator;
        private readonly IMapper _mapper;
        private readonly IUserStorage _storage;
        private readonly IAuthService _auth;

        public CreateUserCommandHandler(IValidator<CreateUserRequest> validator,
            IMapper mapper,
            IUserStorage storage,
            IAuthService auth)
        {
            _validator = validator;
            _mapper = mapper;
            _storage = storage;
            _auth = auth;
        }
        
        public async Task<DomainResponse<LoginResponse>> Handle(CreateUserRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(error => ErrorResponse.Create(error.ErrorMessage))
                    .ToList();
                return DomainResponse<LoginResponse>.CreateError(errors);
            }
            
            var checkResult = await _storage.CheckInfo(request.Name, request.Phone);
            
            if (checkResult != null)
            {
                var response = DomainResponse<LoginResponse>.CreateError();

                if (checkResult.IsPhoneExist)
                {
                    response.Errors.Add(ErrorResponse.Create(ErrorCodes.PhoneIsAlreadyUse));
                }
                
                if (checkResult.IsNameExist)
                {
                    response.Errors.Add(ErrorResponse.Create(ErrorCodes.NameIsAlreadyUse));
                }

                return response;
            }
            
            var user = _mapper.Map<UserModel>(request);
            user.Password = _auth.HashPassword(user.Password);
            await _storage.Create(user);
            
            var result = new LoginResponse()
            {
                UserId = user.Id,
                AccessToken = _auth.GetEncodedJwt(user.Id, user.Name, user.ImageUrl)
            };

            return DomainResponse<LoginResponse>.CreateSuccess(result);
        }
    }
}