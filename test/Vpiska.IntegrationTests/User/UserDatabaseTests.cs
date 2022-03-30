using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using Vpiska.Domain.User.Interfaces;
using Vpiska.Infrastructure.Database;
using Vpiska.IntegrationTests.Common;
using Xunit;

namespace Vpiska.IntegrationTests.User
{
    public sealed class UserDatabaseTests : DatabaseTests
    {
        private readonly Domain.User.User _user1;
        private readonly Domain.User.User _user2;
        private readonly UserEqualityComparer _equalityComparer;
        private readonly IUserRepository _repository;

        public UserDatabaseTests()
        {
            BsonClassMap.RegisterClassMap<Domain.User.User>(options =>
            {
                options.AutoMap();
                options.MapIdMember(x => x.Id);
            });
            
            _user1 = new Domain.User.User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test1",
                Phone = "1234567899",
                PhoneCode = "+7",
                Password = "string"
            };
            _user2 = new Domain.User.User()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test2",
                Phone = "1234567898",
                PhoneCode = "+7",
                Password = "string"
            };
            _equalityComparer = new UserEqualityComparer();
            _repository = new UserRepository(MongoClient, new MongoSettings(DatabaseName));
        }

        [Fact(Timeout = 1000)]
        public async Task Test()
        {
            try
            {
                await InsertTest();
                await GetByFieldOperationsTest();
                await CheckByFieldOperationsTest();
                await UpdateTest();
                await CheckPhoneNameTest();
                await CheckPhoneNameWithEmptyParamsTest();
                await DeleteTest();
                await MongoClient.GetDatabase(DatabaseName).DropCollectionAsync("users");
            }
            catch (Exception)
            {
                await MongoClient.GetDatabase(DatabaseName).DropCollectionAsync("users");
                throw;
            }
        }

        private async Task InsertTest()
        {
            await _repository.InsertAsync(_user1);
            await _repository.InsertAsync(_user2);
        }
        
        private async Task GetByFieldOperationsTest()
        {
            var user1 = await _repository.GetByFieldAsync("_id", _user1.Id);
            var user2 = await _repository.GetByFieldAsync("name", _user2.Name);
            Assert.Equal(_user1, user1, _equalityComparer);
            Assert.Equal(_user2, user2, _equalityComparer);
        }
        
        private async Task CheckByFieldOperationsTest()
        {
            var isExist1 = await _repository.CheckByFieldAsync("_id", _user1.Id);
            Assert.True(isExist1);
            var isExist2 = await _repository.CheckByFieldAsync("name", _user1.Name);
            Assert.True(isExist2);
            var isExist3 = await _repository.CheckByFieldAsync("phone", _user1.Phone);
            Assert.True(isExist3);
            var isExist4 = await _repository.CheckByFieldAsync("password", _user1.Password);
            Assert.True(isExist4);
        }

        private async Task UpdateTest()
        {
            _user1.ImageId = "updatetest";
            await _repository.UpdateUser(_user1.Id, null, null, _user1.ImageId);
            var user = await _repository.GetByFieldAsync("_id", _user1.Id);
            Assert.Equal(_user1, user, _equalityComparer);
            _user1.Name = "testupdatename2";
            _user1.Phone = "77777777777";
            await _repository.UpdateUser(_user1.Id, _user1.Name, _user1.Phone, _user1.ImageId);
            user = await _repository.GetByFieldAsync("_id", _user1.Id);
            Assert.Equal(_user1, user, _equalityComparer);
        }

        private async Task CheckPhoneNameTest()
        {
            var result1 = await _repository.CheckPhoneAndName(_user1.Phone, "qweasd");
            Assert.True(result1.IsPhoneExist);
            Assert.False(result1.IsNameExist);
            var result2 = await _repository.CheckPhoneAndName("123", _user1.Name);
            Assert.False(result2.IsPhoneExist);
            Assert.True(result2.IsNameExist);
            var result3 = await _repository.CheckPhoneAndName(_user1.Phone, _user2.Name);
            Assert.True(result3.IsPhoneExist);
            Assert.True(result3.IsNameExist);
            var result4 = await _repository.CheckPhoneAndName("123654", "qweasdzxc");
            Assert.False(result4.IsPhoneExist);
            Assert.False(result4.IsNameExist);
        }

        private async Task CheckPhoneNameWithEmptyParamsTest()
        {
            var result1 = await _repository.CheckPhoneAndNameWithEmptyParams(null, null);
            Assert.False(result1.IsPhoneExist);
            Assert.False(result1.IsNameExist);
        }

        private async Task DeleteTest()
        {
            var isSuccess1 = await _repository.RemoveByFieldAsync("_id", _user1.Id);
            var isSuccess2 = await _repository.RemoveByFieldAsync("name", _user2.Name);
            Assert.True(isSuccess1);
            Assert.True(isSuccess2);
            var user1 = await _repository.GetByFieldAsync("_id", _user1.Id);
            var user2 = await _repository.GetByFieldAsync("_id", _user2.Id);
            Assert.Null(user1);
            Assert.Null(user2);
        }
    }
}