using System;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Commands.CloseEventCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.Event.Validation
{
    public sealed class CloseEventValidationTests
    {
        private readonly IValidator<CloseEventCommand> _validator;

        public CloseEventValidationTests()
        {
            _validator = new CloseEventValidator();
        }
        
        [Fact]
        public async Task EmptyEventIdTest()
        {
            var command = new CloseEventCommand()
            {
                OwnerId = Guid.NewGuid().ToString()
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
            var command = new CloseEventCommand()
            {
                EventId = "qweasd",
                OwnerId = Guid.NewGuid().ToString()
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
        public async Task EmptyOwnerIdTest()
        {
            var command = new CloseEventCommand()
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
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task InvalidOwnerIdFormatTest()
        {
            var command = new CloseEventCommand()
            {
                OwnerId = "qweasd",
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
                Assert.Contains(Constants.InvalidIdFormat, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllInvalidTest()
        {
            var command = new CloseEventCommand()
            {
                OwnerId = "qweasd",
                EventId = "qweasd"
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
            }
        }
        
        [Fact]
        public async Task AllEmptyTest()
        {
            var command = new CloseEventCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(2, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var command = new CloseEventCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString()
            };
            await _validator.ValidateRequest(command);
        }
    }
}