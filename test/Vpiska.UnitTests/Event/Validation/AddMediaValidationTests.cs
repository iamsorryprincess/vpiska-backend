using System;
using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Commands.AddMediaCommand;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.Event.Validation
{
    public sealed class AddMediaValidationTests
    {
        private readonly IValidator<AddMediaCommand> _validator;

        public AddMediaValidationTests()
        {
            _validator = new AddMediaValidator();
        }
        
        [Fact]
        public async Task EmptyEventIdTest()
        {
            var stream = new MemoryStream();
            var command = new AddMediaCommand()
            {
                OwnerId = Guid.NewGuid().ToString(),
                MediaStream = stream,
                ContentType = "qwe"
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
            finally
            {
                await stream.DisposeAsync();
            }
        }
        
        [Fact]
        public async Task InvalidEventIdFormatTest()
        {
            var stream = new MemoryStream();
            var command = new AddMediaCommand()
            {
                EventId = "qweasd",
                MediaStream = stream,
                ContentType = "qwe"
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
            finally
            {
                await stream.DisposeAsync();
            }
        }

        [Fact]
        public async Task EmptyContentTypeTest()
        {
            var stream = new MemoryStream();
            var command = new AddMediaCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                MediaStream = stream
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.MediaContentTypeIsEmpty, ex.ErrorsCodes);
            }
            finally
            {
                await stream.DisposeAsync();
            }
        }
        
        [Fact]
        public async Task EmptyContentStreamTest()
        {
            var command = new AddMediaCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                ContentType = "qwesad"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.MediaIsEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllEmptyTest()
        {
            var command = new AddMediaCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(3, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.MediaIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.MediaContentTypeIsEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var stream = new MemoryStream();
            var command = new AddMediaCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                MediaStream = stream,
                ContentType = "qwe"
            };
            try
            {
                await _validator.ValidateRequest(command);
            }
            finally
            {
                await stream.DisposeAsync();
            }
        }
    }
}