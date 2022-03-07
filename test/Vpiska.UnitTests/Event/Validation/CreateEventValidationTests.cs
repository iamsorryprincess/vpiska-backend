using System;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Commands.CreateEventCommand;
using Vpiska.Domain.Event.Models;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.Event.Validation
{
    public sealed class CreateEventValidationTests
    {
        private readonly IValidator<CreateEventCommand> _validator;

        public CreateEventValidationTests()
        {
            _validator = new CreateEventValidator();
        }

        [Fact]
        public async Task EmptyOwnerIdTest()
        {
            var command = new CreateEventCommand()
            {
                Name = "test",
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
        public async Task InvalidOwnerIdFormatTest()
        {
            var command = new CreateEventCommand()
            {
                OwnerId = "qwe",
                Name = "test",
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
        public async Task EmptyNameTest()
        {
            var command = new CreateEventCommand()
            {
                OwnerId = Guid.NewGuid().ToString(),
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
                Assert.Contains(Constants.NameIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task EmptyAddressTest()
        {
            var command = new CreateEventCommand()
            {
                OwnerId = Guid.NewGuid().ToString(),
                Name = "test",
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
            var command = new CreateEventCommand()
            {
                OwnerId = Guid.NewGuid().ToString(),
                Name = "test",
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
            var command = new CreateEventCommand();
            try
            {
                await _validator.ValidateRequest(command);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(4, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.IdIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.NameIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.AddressIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var command = new CreateEventCommand()
            {
                OwnerId = Guid.NewGuid().ToString(),
                Name = "test",
                Address = "test",
                Coordinates = new CoordinatesDto()
                {
                    X = 123,
                    Y = 123
                }
            };
            await _validator.ValidateRequest(command);
        }
    }
}