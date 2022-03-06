using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.User;
using Vpiska.Domain.User.Queries.CheckCodeQuery;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.User.Validation
{
    public sealed class CheckCodeValidationTests
    {
        private readonly IValidator<CheckCodeQuery> _validator;

        public CheckCodeValidationTests()
        {
            _validator = new CheckCodeValidator();
        }
        
        [Fact]
        public async Task InvalidPhoneFormatTest()
        {
            var query = new CheckCodeQuery()
            {
                Phone = "qweasd",
                Code = 111112
            };
            try
            {
                await _validator.ValidateRequest(query);
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
            var query = new CheckCodeQuery()
            {
                Code = 111112
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.PhoneIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task EmptyCodeTest()
        {
            var query = new CheckCodeQuery()
            {
                Phone = "9374113516"
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CodeIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task InvalidMinCodeLengthTest()
        {
            var query = new CheckCodeQuery()
            {
                Phone = "9374113516",
                Code = 11122
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CodeLengthInvalid, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task InvalidMaxCodeLengthTest()
        {
            var query = new CheckCodeQuery()
            {
                Phone = "9374113516",
                Code = 111222323
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CodeLengthInvalid, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllEmptyTest()
        {
            var query = new CheckCodeQuery();
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PhoneIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.CodeIsEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllInvalidTest()
        {
            var query = new CheckCodeQuery()
            {
                Phone = "qwe",
                Code = 123
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.PhoneRegexInvalid, ex.ErrorsCodes);
                Assert.Contains(Constants.CodeLengthInvalid, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var query = new CheckCodeQuery()
            {
                Phone = "9374113516",
                Code = 111112
            };
            await _validator.ValidateRequest(query);
        }
    }
}