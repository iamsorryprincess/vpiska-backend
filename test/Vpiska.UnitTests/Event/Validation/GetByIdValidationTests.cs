using System;
using System.Threading.Tasks;
using FluentValidation;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event;
using Vpiska.Domain.Event.Queries.GetByIdQuery;
using Xunit;
using Xunit.Sdk;
using ValidationException = Vpiska.Domain.Common.Exceptions.ValidationException;

namespace Vpiska.UnitTests.Event.Validation
{
    public sealed class GetByIdValidationTests
    {
        private readonly IValidator<GetByIdQuery> _validator;

        public GetByIdValidationTests()
        {
            _validator = new GetByIdValidator();
        }
        
        [Fact]
        public async Task EmptyOwnerIdTest()
        {
            var query = new GetByIdQuery();
            try
            {
                await _validator.ValidateRequest(query);
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
            var query = new GetByIdQuery()
            {
                EventId = "qewesd"
            };
            try
            {
                await _validator.ValidateRequest(query);
                throw new TestClassException("must be validation exception");
            }
            catch (ValidationException ex)
            {
                Assert.Single(ex.ErrorsCodes);
                Assert.Contains(Constants.InvalidIdFormat, ex.ErrorsCodes);
            }
        }

        [Fact]
        public async Task ValidTest()
        {
            var query = new GetByIdQuery() { EventId = Guid.NewGuid().ToString() };
            await _validator.ValidateRequest(query);
        }
    }
}