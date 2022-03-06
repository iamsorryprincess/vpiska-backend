using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Commands.LoginUserCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.User.Validation
{
    public sealed class LoginUserValidationTests
    {
        private readonly IValidator<LoginUserCommand> _validator;

        public LoginUserValidationTests()
        {
            _validator = new LoginUserValidator();
        }
        
        [Fact]
        public async Task InvalidPhoneFormatTest()
        {
            var command = new LoginUserCommand()
            {
                Phone = "qweasd",
                Password = "string"
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
            var command = new LoginUserCommand()
            {
                Password = "string"
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
        public async Task EmptyPasswordTest()
        {
            var command = new LoginUserCommand()
            {
                Phone = "9374113516"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task PasswordLengthInvalidTest()
        {
            var command = new LoginUserCommand()
            {
                Phone = "9374113516",
                Password = "strin"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordLengthInvalid, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task AllEmptyTest()
        {
            var command = new LoginUserCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PhoneIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task AllInvalidTest()
        {
            var command = new LoginUserCommand()
            {
                Phone = "asd",
                Password = "123"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PhoneRegexInvalid, ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordLengthInvalid, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var command = new LoginUserCommand()
            {
                Phone = "9374113516",
                Password = "qweasd"
            };
            await _validator.ValidateRequest(command);
        }
    }
}