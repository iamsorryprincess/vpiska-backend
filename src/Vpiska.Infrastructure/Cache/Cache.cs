using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vpiska.Domain.Interfaces;

namespace Vpiska.Infrastructure.Cache
{
    internal sealed class Cache<T> : ICache<T> where T : class, new()
    {
        private readonly ConcurrentDictionary<string, T> _dictionary;

        public Cache()
        {
            _dictionary = new ConcurrentDictionary<string, T>();
        }

        public Task<T> Get(string key) => Task.FromResult(_dictionary.TryGetValue(key, out var data) ? data : null);

        public Task<bool> Set(string key, T data) => Task.FromResult(_dictionary.TryAdd(key, data));

        public Task<bool> Remove(string key) => Task.FromResult(_dictionary.TryRemove(key, out _));
    }
}