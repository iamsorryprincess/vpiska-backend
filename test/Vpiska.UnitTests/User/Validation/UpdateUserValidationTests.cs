using System;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Commands.UpdateUserCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.User.Validation
{
    public sealed class UpdateUserValidationTests
    {
        private readonly IValidator<UpdateUserCommand> _validator;

        public UpdateUserValidationTests()
        {
            _validator = new UpdateUserValidator();
        }
        
        [Fact]
        public async Task EmptyIdTest()
        {
            var command = new UpdateUserCommand();
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
            var command = new UpdateUserCommand()
            {
                Id = "qewasd"
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
        public async Task EmptyPhoneIsValidTest()
        {
            var command = new UpdateUserCommand()
            {
                Id = Guid.NewGuid().ToString()
            };
            await _validator.ValidateRequest(command);
        }
        
        [Fact]
        public async Task InvalidPhoneFormatTest()
        {
            var command = new UpdateUserCommand()
            {
                Id = Guid.NewGuid().ToString(),
                Phone = "qweasd"
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
        public async Task AllInvalidTest()
        {
            var command = new UpdateUserCommand()
            {
                Id = "qwe",
                Phone = "asd"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.InvalidIdFormat, ex.ErrorsCodes);
                Assert.Contains(Constants.PhoneRegexInvalid, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task AllValidTest()
        {
            var command = new UpdateUserCommand()
            {
                Id = Guid.NewGuid().ToString(),
                Phone = "9374113516",
                Name = "qwe"
            };
            await _validator.ValidateRequest(command);
        }
    }
}