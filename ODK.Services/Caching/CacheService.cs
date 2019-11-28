using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ODK.Core;

namespace ODK.Services.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<T>>> GetOrSetVersionedCollection<T>(Func<Task<IReadOnlyCollection<T>>> getter,
            Func<Task<long>> getVersion, long? currentVersion)
        {
            object instanceKey = "";

            long? version = TryGetCachedVersion<T>(instanceKey);
            if (version == null)
            {
                version = await getVersion();
                SetVersion<T>(version.Value, instanceKey);
            }

            if (currentVersion == version)
            {
                return new VersionedServiceResult<IReadOnlyCollection<T>>(version.Value);
            }

            IReadOnlyCollection<T> collection = await GetOrSet(getter, instanceKey, version, null);
            return new VersionedServiceResult<IReadOnlyCollection<T>>(version.Value, collection);
        }

        public async Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T>> getter, object instanceKey, long? currentVersion) where T : class, IVersioned
        {
            long? version = TryGetCachedVersion<T>(instanceKey);

            T item = null;
            if (version == null)
            {
                item = await getter();
                version = item.Version;
                SetVersion<T>(version.Value, instanceKey);
            }

            if (currentVersion == version)
            {
                return new VersionedServiceResult<T>(version.Value);
            }

            item ??= await GetOrSet(getter, instanceKey, version.Value, null);

            return new VersionedServiceResult<T>(version.Value, item);
        }

        public void RemoveVersionedItem<T>(object instanceKey)
        {
            string versionKey = GetVersionKey<T>(instanceKey);
            string key = GetKey<T>(instanceKey);

            _cache.Remove(versionKey);
            _cache.Remove(key);
        }

        private static string GetKey<T>(object instanceKey)
        {
            return typeof(T).FullName + instanceKey;
        }

        private static string GetVersionKey<T>(object instanceKey)
        {
            return GetKey<T>(instanceKey) + ".Version";
        }

        private async Task<T> GetOrSet<T>(Func<Task<T>> getter, object instanceKey, long? version, TimeSpan? expiresIn)
        {
            string key = GetKey<T>(instanceKey);
            if (TryGetValue(key, version, out T value))
            {
                return value;
            }

            value = await getter();
            Set(value, instanceKey, version, expiresIn);
            return value;
        }

        private void Set<T>(T value, object instanceKey, long? version, TimeSpan? expiresIn)
        {
            CacheItem<T> cacheItem = new CacheItem<T>
            {
                Value = value,
                Version = version
            };

            string key = GetKey<T>(instanceKey);
            if (expiresIn != null)
            {
                _cache.Set(key, cacheItem, expiresIn.Value);
            }
            else
            {
                _cache.Set(key, cacheItem);
            }
        }

        private void SetVersion<T>(long version, object instanceKey)
        {
            string key = GetVersionKey<T>(instanceKey);
            _cache.Set(key, version);
        }

        private bool TryGetValue<T>(string key, long? version, out T value)
        {
            if (_cache.TryGetValue(key, out CacheItem<T> cacheItem) && (version == null || version == cacheItem.Version))
            {
                value = cacheItem.Value;
                return true;
            }

            value = default;
            return false;
        }

        private long? TryGetCachedVersion<T>(object instanceKey)
        {
            string versionKey = GetVersionKey<T>(instanceKey);
            if (_cache.TryGetValue(versionKey, out long value))
            {
                return value;
            }

            return null;
        }
    }
}
