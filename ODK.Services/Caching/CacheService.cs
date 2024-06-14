using Microsoft.Extensions.Caching.Memory;
using ODK.Core;

namespace ODK.Services.Caching;

public class CacheService : ICacheService
{
    private const string CollectionInstanceKey = "";

    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetOrSetItem<T>(Func<Task<T?>> getter, object instanceKey) where T : class
    {
        return await GetOrSet(getter, instanceKey, null, null);
    }

    public async Task<T?> GetOrSetItem<T>(Func<Task<T?>> getter, object instanceKey, TimeSpan lifetime) where T : class
    {
        return await GetOrSet(getter, instanceKey, null, lifetime);
    }
    
    public async Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T?>> getter, object key,
        long? currentVersion) where T : class, IVersioned
    {
        return await GetOrSetVersionedItem(getter, t => Task.FromResult(t?.Version ?? 0), key, currentVersion);
    }

    public async Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T?>> getter, Func<T, Task<long>> getVersion,
        object key, long? currentVersion) where T : class
    {
        long? version = TryGetCachedVersion<T>(key);

        T? item = null;
        if (version == null)
        {
            item = await getter();
            if (item == null)
            {
                return new VersionedServiceResult<T>(0, null);
            }

            version = await getVersion(item);
            SetVersion<T>(version.Value, key);
        }

        if (currentVersion == version)
        {
            return new VersionedServiceResult<T>(version.Value);
        }

        item ??= await GetOrSet(getter, key, version.Value, null);

        return new VersionedServiceResult<T>(version.Value, item);
    }

    public void RemoveVersionedCollection<T>(object? key = null)
    {
        RemoveVersionedItem<T>(key ?? CollectionInstanceKey);
    }

    public void RemoveVersionedItem<T>(object instanceKey)
    {
        string versionKey = GetVersionKey<T>(instanceKey);
        string key = GetKey<T>(instanceKey);

        _cache.Remove(versionKey);
        _cache.Remove(key);
    }

    public void UpdateItem<T>(T item, object instanceKey)
    {
        Set(item, instanceKey, null, null);
    }

    public void UpdatedVersionedCollection<T>(IReadOnlyCollection<T> collection, long version, object? key = null)
    {
        SetVersion<T>(version, key ?? CollectionInstanceKey);
        Set(collection, key ?? CollectionInstanceKey, version, null);
    }

    public void UpdatedVersionedItem<T>(T item, object instanceKey) where T : IVersioned
    {
        SetVersion<T>(item.Version, instanceKey);
        Set(item, instanceKey, item.Version, null);
    }

    private static string GetKey<T>(object instanceKey)
    {
        return typeof(T).FullName + instanceKey;
    }

    private static string GetVersionKey<T>(object instanceKey)
    {
        return GetKey<T>(instanceKey) + ".Version";
    }

    private async Task<T?> GetOrSet<T>(Func<Task<T?>> getter, object instanceKey, long? version, TimeSpan? expiresIn) where T : class
    {
        string key = GetKey<T>(instanceKey);
        if (TryGetValue(key, version, out T? value))
        {
            return value!;
        }

        value = await getter();
        Set(value, instanceKey, version, expiresIn);
        return value;
    }

    private void Set<T>(T value, object instanceKey, long? version, TimeSpan? expiresIn)
    {
        CacheItem<T> cacheItem = new CacheItem<T>(value, version);

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

    private bool TryGetValue<T>(string key, long? version, out T? value) where T : class
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
