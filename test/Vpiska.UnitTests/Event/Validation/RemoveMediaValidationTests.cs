using System;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Commands.RemoveMediaCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.Event.Validation
{
    public sealed class RemoveMediaValidationTests
    {
        private readonly IValidator<RemoveMediaCommand> _validator;

        public RemoveMediaValidationTests()
        {
            _validator = new RemoveMediaValidator();
        }
        
        [Fact]
        public async Task EmptyEventIdTest()
        {
            var command = new RemoveMediaCommand()
            {
                MediaId = "qwe"
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
        public async Task InvalidEventIdFormatTest()
        {
            var command = new RemoveMediaCommand()
            {
                EventId = "qweasd",
                MediaId = "qwe"
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
        public async Task EmptyMediaIdTest()
        {
            var command = new RemoveMediaCommand()
            {
                EventId = Guid.NewGuid().ToString()
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.MediaIdIsEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllEmptyTest()
        {
            var command = new RemoveMediaCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.MediaIdIsEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var command = new RemoveMediaCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                MediaId = "qwesad"
            };
            await _validator.ValidateRequest(command);
        }
    }
}