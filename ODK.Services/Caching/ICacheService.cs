using ODK.Core;

namespace ODK.Services.Caching;

public interface ICacheService
{
    Task<T?> GetOrSetItem<T>(Func<Task<T?>> getter, object instanceKey) where T : class;

    Task<T?> GetOrSetItem<T>(Func<Task<T?>> getter, object instanceKey, TimeSpan lifetime) where T : class;
    
    Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T?>> getter, object key, 
        long? currentVersion) where T : class, IVersioned;
    
    void RemoveVersionedCollection<T>(object? key = null);

    void RemoveVersionedItem<T>(object instanceKey);

    void UpdateItem<T>(T item, object instanceKey);

    void UpdatedVersionedCollection<T>(IReadOnlyCollection<T> collection, long version, object? key = null);

    void UpdatedVersionedItem<T>(T item, object instanceKey) where T : IVersioned;
}
