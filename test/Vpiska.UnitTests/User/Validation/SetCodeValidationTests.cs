using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Commands.SetCodeCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.User.Validation
{
    public sealed class SetCodeValidationTests
    {
        private readonly IValidator<SetCodeCommand> _validator;

        public SetCodeValidationTests()
        {
            _validator = new SetCodeValidator();
        }
        
        [Fact]
        public async Task InvalidPhoneFormatTest()
        {
            var command = new SetCodeCommand()
            {
                Phone = "qweasd",
                Token = "qweasd"
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
            var command = new SetCodeCommand()
            {
                Token = "asd"
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
        public async Task EmptyTokenTest()
        {
            var command = new SetCodeCommand()
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
                Assert.Contains(Constants.TokenIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task AllEmptyTest()
        {
            var command = new SetCodeCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PhoneIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.TokenIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task AllValidTest()
        {
            var command = new SetCodeCommand()
            {
                Phone = "9374113516",
                Token = "qewasd"
            };
            await _validator.ValidateRequest(command);
        }
    }
}