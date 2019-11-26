using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ODK.Core.Caching;

namespace ODK.Services.Caching
{
    public class Cache : ICache
    {
        private readonly IMemoryCache _cache;

        public Cache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<IReadOnlyCollection<T>> GetOrSetCollection<T>(Func<Task<IReadOnlyCollection<T>>> getter)
        {
            string key = GetKey<T>();
            return await GetOrSet(key, getter);
        }

        public void Remove<T>()
        {
            string key = GetKey<T>();
            _cache.Remove(key);
        }

        public IReadOnlyCollection<T> TryGetCollection<T>()
        {
            string key = GetKey<T>();
            _cache.TryGetValue(key, out IReadOnlyCollection<T> value);
            return value;
        }

        private static string GetKey<T>()
        {
            return typeof(T).FullName;
        }

        private async Task<T> GetOrSet<T>(string key, Func<Task<T>> getter)
        {
            if (_cache.TryGetValue(key, out T value))
            {
                return value;
            }

            value = await getter();
            _cache.Set(key, value, TimeSpan.FromHours(1));
            return value;
        }
    }
}
