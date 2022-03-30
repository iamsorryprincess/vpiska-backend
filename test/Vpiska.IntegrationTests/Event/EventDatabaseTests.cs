using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Models;
using Vpiska.Infrastructure.Database;
using Vpiska.IntegrationTests.Common;
using Xunit;

namespace Vpiska.IntegrationTests.Event
{
    public sealed class EventDatabaseTests : DatabaseTests
    {
        private readonly Domain.Event.Event _event1;
        private readonly Domain.Event.Event _event2;
        private readonly EventEqualityComparer _equalityComparer;
        private readonly IEventRepository _repository;

        public EventDatabaseTests()
        {
            BsonClassMap.RegisterClassMap<Domain.Event.Event>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });

            _event1 = new Domain.Event.Event()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
                Name = "test1",
                Address = "address1",
                Coordinates = new Coordinates()
                {
                    X = 11.111,
                    Y = 22.222
                }
            };

            _event2 = new Domain.Event.Event()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
                Name = "test2",
                Address = "address2",
                Coordinates = new Coordinates()
                {
                    X = 33.333,
                    Y = 44.444
                }
            };

            _equalityComparer = new EventEqualityComparer();
            _repository = new EventRepository(MongoClient, new MongoSettings(DatabaseName));
        }

        [Fact(Timeout = 1000)]
        public async Task Test()
        {
            try
            {
                await InsertTest();
                await GetByFieldOperationsTest();
                await CheckByFieldOperationsTest();
                await MediaLinksTest();
                await MongoClient.GetDatabase(DatabaseName).DropCollectionAsync("events");
            }
            catch (Exception)
            {
                await MongoClient.GetDatabase(DatabaseName).DropCollectionAsync("events");
                throw;
            }
        }
        
        private async Task InsertTest()
        {
            await _repository.InsertAsync(_event1);
            await _repository.InsertAsync(_event2);
        }
        
        private async Task GetByFieldOperationsTest()
        {
            var event1 = await _repository.GetByFieldAsync("_id", _event1.Id);
            var event2 = await _repository.GetByFieldAsync("name", _event2.Name);
            Assert.Equal(_event1, event1, _equalityComparer);
            Assert.Equal(_event2, event2, _equalityComparer);
        }
        
        private async Task CheckByFieldOperationsTest()
        {
            var isExist1 = await _repository.CheckByFieldAsync("_id", _event1.Id);
            Assert.True(isExist1);
            var isExist2 = await _repository.CheckByFieldAsync("name", _event1.Name);
            Assert.True(isExist2);
            var isExist3 = await _repository.CheckByFieldAsync("address", _event1.Address);
            Assert.True(isExist3);
            var isExist4 = await _repository.CheckByFieldAsync("ownerId", _event1.OwnerId);
            Assert.True(isExist4);
        }

        private async Task MediaLinksTest()
        {
            var isSuccess1 = await _repository.AddMediaLink(_event1.Id, "testmedia1");
            Assert.True(isSuccess1);
            var event1 = await _repository.GetByFieldAsync("_id", _event1.Id);
            Assert.Contains("testmedia1", event1.MediaLinks);
            var isSuccess2 = await _repository.AddMediaLink(Guid.NewGuid().ToString(), "testmedia1");
            Assert.False(isSuccess2);
            var isSuccess3 = await _repository.RemoveMediaLink(_event1.Id, "testmedia1");
            Assert.True(isSuccess3);
        }
    }
}