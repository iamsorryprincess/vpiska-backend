using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Requests;
using Vpiska.Domain.Responses;
using Vpiska.Domain.Validation;

namespace Vpiska.Domain.CommandHandlers
{
    public sealed class NotificationsCommandHandler : ICommandHandler<CodeRequest>
    {
        private static readonly Random Random = new Random();
        
        private readonly IValidator<CodeRequest> _validator;
        private readonly IUserStorage _storage;
        private readonly INotificationService _notificationService;

        public NotificationsCommandHandler(IValidator<CodeRequest> validator,
            IUserStorage storage,
            INotificationService notificationService)
        {
            _validator = validator;
            _storage = storage;
            _notificationService = notificationService;
        }

        public async Task<DomainResponse> Handle(CodeRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(error => ErrorResponse.Create(error.ErrorMessage))
                    .ToList();
                return DomainResponse.CreateError(errors);
            }
            
            var code = Random.Next(111111, 777777);
            var isSuccess = await _storage.SetVerificationCode(request.Phone, code);

            if (!isSuccess)
            {
                var response = DomainResponse.CreateError();
                response.Errors.Add(ErrorResponse.Create(ErrorCodes.UserByPhoneNotFound));
                return response;
            }

            await _notificationService.SendVerificationCode(code);
            return DomainResponse.CreateSuccess();
        }
    }
}