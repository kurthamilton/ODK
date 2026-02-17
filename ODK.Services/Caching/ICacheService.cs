using ODK.Core;

namespace ODK.Services.Caching;

public interface ICacheService
{
    Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T?>> getter, object key,
        long? currentVersion) where T : class, IVersioned;

    void RemoveVersionedItem<T>(object instanceKey);
}