using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Commands.CreateUserCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.User.Validation
{
    public sealed class CreateUserValidationTests
    {
        private readonly IValidator<CreateUserCommand> _validator;

        public CreateUserValidationTests()
        {
            _validator = new CreateUserValidator();
        }

        [Fact]
        public async Task InvalidPhoneFormatTest()
        {
            var command = new CreateUserCommand()
            {
                Phone = "qweasd",
                Name = "string",
                Password = "string",
                ConfirmPassword = "string"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.PhoneRegexInvalid, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task EmptyPhoneTest()
        {
            var command = new CreateUserCommand()
            {
                Name = "string",
                Password = "string",
                ConfirmPassword = "string"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.PhoneIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task EmptyNameTest()
        {
            var command = new CreateUserCommand()
            {
                Phone = "9374113516",
                Password = "string",
                ConfirmPassword = "string"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.NameIsEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task EmptyPasswordTest()
        {
            var command = new CreateUserCommand()
            {
                Phone = "9374113516",
                Name = "string",
                ConfirmPassword = "string"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PasswordIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.ConfirmPasswordInvalid, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task ConfirmPasswordInvalidTest()
        {
            var command = new CreateUserCommand()
            {
                Phone = "9374113516",
                Password = "strings",
                Name = "string",
                ConfirmPassword = "string"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.ConfirmPasswordInvalid, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task PasswordLengthInvalidTest()
        {
            var command = new CreateUserCommand()
            {
                Phone = "9374113516",
                Password = "strin",
                Name = "string",
                ConfirmPassword = "string"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PasswordLengthInvalid, ex.ErrorsCodes);
                Assert.Contains(Constants.ConfirmPasswordInvalid, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllEmptyTest()
        {
            var command = new CreateUserCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(3, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PhoneIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.NameIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task AllInvalidTest()
        {
            var command = new CreateUserCommand()
            {
                Phone = "qwe",
                Password = "123",
                ConfirmPassword = "231"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(4, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PhoneRegexInvalid, ex.ErrorsCodes);
                Assert.Contains(Constants.NameIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordLengthInvalid, ex.ErrorsCodes);
                Assert.Contains(Constants.ConfirmPasswordInvalid, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var command = new CreateUserCommand()
            {
                Phone = "9374113516",
                Name = "string",
                Password = "string",
                ConfirmPassword = "string"
            };
            await _validator.ValidateRequest(command);
        }
    }
}