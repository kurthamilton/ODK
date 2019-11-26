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
            return await GetOrSet(getter, null);
        }

        public async Task<IReadOnlyCollection<T>> GetOrSetCollection<T>(Func<Task<IReadOnlyCollection<T>>> getter, int version)
        {
            return await GetOrSet(getter, version);
        }

        public async Task<int> GetOrSetVersion<T>(Func<Task<int>> getter)
        {
            string key = $"{GetKey<T>()}.Version";
            if (_cache.TryGetValue(key, out int version))
            {
                return version;
            }

            version = await getter();
            _cache.Set(key, version, TimeSpan.FromMinutes(5));
            return version;
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

        private async Task<T> GetOrSet<T>(Func<Task<T>> getter, int? version)
        {
            if (TryGetValue(version, out T value))
            {
                return value;
            }

            value = await getter();
            Set(value, version);
            return value;
        }

        private void Set<T>(T value, int? version)
        {
            CacheItem<T> cacheItem = new CacheItem<T>
            {
                Value = value,
                Version = version
            };

            string key = GetKey<T>();

            _cache.Set(key, cacheItem, TimeSpan.FromHours(1));
        }

        private bool TryGetValue<T>(int? version, out T value)
        {
            string key = GetKey<T>();
            if (_cache.TryGetValue(key, out CacheItem<T> cacheItem) && (version == null || version == cacheItem.Version))
            {
                value = cacheItem.Value;
                return true;
            }

            value = default;
            return false;
        }
    }
}
