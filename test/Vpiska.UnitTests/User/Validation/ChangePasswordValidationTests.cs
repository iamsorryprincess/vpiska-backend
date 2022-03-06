using System;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Commands.ChangePasswordCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.User.Validation
{
    public sealed class ChangePasswordValidationTests
    {
        private readonly IValidator<ChangePasswordCommand> _validator;

        public ChangePasswordValidationTests()
        {
            _validator = new ChangePasswordValidator();
        }
        
        [Fact]
        public async Task EmptyIdTest()
        {
            var command = new ChangePasswordCommand()
            {
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
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task InvalidIdFormatTest()
        {
            var command = new ChangePasswordCommand()
            {
                Id = "qewasd",
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
                Assert.Contains(Constants.InvalidIdFormat, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task EmptyPasswordTest()
        {
            var command = new ChangePasswordCommand()
            {
                Id = Guid.NewGuid().ToString(),
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
            var command = new ChangePasswordCommand()
            {
                Id = Guid.NewGuid().ToString(),
                Password = "strings",
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
            var command = new ChangePasswordCommand()
            {
                Id = Guid.NewGuid().ToString(),
                Password = "strin",
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
            var command = new ChangePasswordCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task AllInvalidTest()
        {
            var command = new ChangePasswordCommand()
            {
                Id = "qweasd",
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
                Assert.Equal(3, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.InvalidIdFormat, ex.ErrorsCodes);
                Assert.Contains(Constants.PasswordLengthInvalid, ex.ErrorsCodes);
                Assert.Contains(Constants.ConfirmPasswordInvalid, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var command = new ChangePasswordCommand()
            {
                Id = Guid.NewGuid().ToString(),
                Password = "string",
                ConfirmPassword = "string"
            };
            await _validator.ValidateRequest(command);
        }
    }
}