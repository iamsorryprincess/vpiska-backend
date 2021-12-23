using System.Collections.Generic;
using MongoDB.Driver;
using Vpiska.Api.Requests;
using Vpiska.Api.Requests.User;
using Vpiska.Domain.Models;

namespace Vpiska.Api.Extensions
{
    public static class MongoExtensions
    {
        public static IMongoCollection<User> GetUsers(this IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            return database.GetCollection<User>("users");
        }

        public static FilterDefinition<User> CreateUserIdFilter(this string id) =>
            Builders<User>.Filter.Eq(x => x.Id, id);

        public static FilterDefinition<User> CreatePhoneFilter(this string phone) =>
            Builders<User>.Filter.Eq(x => x.Phone, phone);

        public static FilterDefinition<User> CreateNameFilter(this string name) =>
            Builders<User>.Filter.Eq(x => x.Name, name);

        public static FilterDefinition<TModel> Or<TModel>(this FilterDefinition<TModel> filterDefinition,
            FilterDefinition<TModel> anotherFilterDefinition) =>
            Builders<TModel>.Filter.Or(filterDefinition, anotherFilterDefinition);

        public static FilterDefinition<TModel> And<TModel>(this FilterDefinition<TModel> filterDefinition,
            FilterDefinition<TModel> anotherFilterDefinition) =>
            Builders<TModel>.Filter.And(filterDefinition, anotherFilterDefinition);
        
        public static FilterDefinition<User> CreateCheckFilter(this UpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) && !string.IsNullOrWhiteSpace(request.Phone))
            {
                return request.Name.CreateNameFilter();
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.Phone))
            {
                return request.Phone.CreatePhoneFilter();
            }

            if (string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.Phone))
            {
                return request.Phone.CreatePhoneFilter().Or(request.Name.CreateNameFilter());
            }

            return null;
        }

        public static UpdateDefinition<User> CreateUpdateDefinition(this UpdateUserRequest request, string imageId)
        {
            var updates = new List<UpdateDefinition<User>>();

            if (!string.IsNullOrWhiteSpace(request.Name))
                updates.Add(Builders<User>.Update.Set(x => x.Name, request.Name));
            if (!string.IsNullOrWhiteSpace(request.Phone))
                updates.Add(Builders<User>.Update.Set(x => x.Phone, request.Phone));
            if (!string.IsNullOrWhiteSpace(imageId))
                updates.Add(Builders<User>.Update.Set(x => x.ImageId, imageId));

            return Builders<User>.Update.Combine(updates);
        }
    }
}