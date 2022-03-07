using System;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Commands.ChangeLocationCommand;
using Vpiska.Domain.Event.Models;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.Event.Validation
{
    public sealed class UpdateEventValidationTests
    {
        private readonly IValidator<ChangeLocationCommand> _validator;

        public UpdateEventValidationTests()
        {
            _validator = new ChangeLocationValidator();
        }
        
        [Fact]
        public async Task EmptyIdTest()
        {
            var command = new ChangeLocationCommand()
            {
                Address = "test",
                Coordinates = new CoordinatesDto()
                {
                    X = 0,
                    Y = 0
                }
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
            var command = new ChangeLocationCommand()
            {
                EventId = "qweasd",
                Address = "test",
                Coordinates = new CoordinatesDto()
                {
                    X = 0,
                    Y = 0
                }
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
        public async Task EmptyAddressTest()
        {
            var command = new ChangeLocationCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                Coordinates = new CoordinatesDto()
                {
                    X = 0,
                    Y = 0
                }
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.AddressIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task EmptyCoordinatesTest()
        {
            var command = new ChangeLocationCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                Address = "test"
            };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }

            command.Coordinates = new CoordinatesDto() { X = 23 };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }
            
            command.Coordinates = new CoordinatesDto() { Y = 23 };
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllEmptyTest()
        {
            var command = new ChangeLocationCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(3, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.AddressIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var command = new ChangeLocationCommand()
            {
                EventId = Guid.NewGuid().ToString(),
                Address = "qweasd",
                Coordinates = new CoordinatesDto()
                {
                    X = 0,
                    Y = 0
                }
            };
            await _validator.ValidateRequest(command);
        }
    }
}