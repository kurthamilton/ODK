using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core;

namespace ODK.Services.Caching
{
    public interface ICacheService
    {
        Task<VersionedServiceResult<IReadOnlyCollection<T>>> GetOrSetVersionedCollection<T>(Func<Task<IReadOnlyCollection<T>>> getter, Func<Task<long>> getVersion, long? currentVersion);

        Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T>> getter, object instanceKey, long? currentVersion) where T : class, IVersioned;

        void RemoveVersionedCollection<T>();

        void RemoveVersionedItem<T>(object instanceKey);
    }
}
