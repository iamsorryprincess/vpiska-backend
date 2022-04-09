using System.Threading.Tasks;
using MongoDB.Driver;
using RtclightWeb.Models;

namespace RtclightWeb.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
        }

        public Task<User> GetByName(string name)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Name, name);
            return GetByFilter(filter);
        }

        public Task<User> GetByCredentials(string name, string password)
        {
            var filter = Builders<User>.Filter.And(Builders<User>.Filter.Eq(x => x.Name, name),
                Builders<User>.Filter.Eq(x => x.Password, password));
            return GetByFilter(filter);
        }
        
        public async Task<string> Insert(User user)
        {
            await _users.InsertOneAsync(user);
            return user.Id;
        }

        private async Task<User> GetByFilter(FilterDefinition<User> filter)
        {
            var cursor = await _users.FindAsync(filter);
            var result = await cursor.FirstOrDefaultAsync();
            return result;
        }
    }
}