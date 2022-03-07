using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Models;
using Vpiska.Domain.Event.Queries.GetEventsQuery;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.Event.Validation
{
    public sealed class GetEventsValidationTests
    {
        private readonly IValidator<GetEventsQuery> _validator;

        public GetEventsValidationTests()
        {
            _validator = new GetEventsValidator();
        }

        [Fact]
        public async Task HorizontalRangeEmptyTest()
        {
            var query = new GetEventsQuery()
            {
                VerticalRange = 123,
                Coordinates = new CoordinatesDto()
                {
                    X = 0,
                    Y = 0
                }
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.HorizontalRangeIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task VerticalRangeEmptyTest()
        {
            var query = new GetEventsQuery()
            {
                HorizontalRange = 123,
                Coordinates = new CoordinatesDto()
                {
                    X = 0,
                    Y = 0
                }
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.VerticalRangeIsEmpty, ex.ErrorsCodes);
            }
        }
        
        [Fact]
        public async Task EmptyCoordinatesTest()
        {
            var query = new GetEventsQuery()
            {
                HorizontalRange = 0,
                VerticalRange = 0
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }

            query.Coordinates = new CoordinatesDto() { X = 23 };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }
            
            query.Coordinates = new CoordinatesDto() { Y = 23 };
            try
            {
                await _validator.ValidateRequest(query);
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
            var query = new GetEventsQuery();
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Equal(3, ex.ErrorsCodes.Length);
                Assert.Contains(Constants.HorizontalRangeIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.VerticalRangeIsEmpty, ex.ErrorsCodes);
                Assert.Contains(Constants.CoordinatesAreEmpty, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task AllValidTest()
        {
            var query = new GetEventsQuery()
            {
                VerticalRange = 0,
                HorizontalRange = 0,
                Coordinates = new CoordinatesDto()
                {
                    X = 0,
                    Y = 0
                }
            };
            await _validator.ValidateRequest(query);
        }
    }
}