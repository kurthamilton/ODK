using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core;

namespace ODK.Services.Caching
{
    public interface ICacheService
    {
        Task<T> GetOrSetItem<T>(Func<Task<T>> getter, object instanceKey);

        Task<VersionedServiceResult<IReadOnlyCollection<T>>> GetOrSetVersionedCollection<T>(Func<Task<IReadOnlyCollection<T>>> getter, 
            Func<Task<long>> getVersion, long? currentVersion, object key = null);

        Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T>> getter, object key, 
            long? currentVersion) where T : class, IVersioned;

        Task<VersionedServiceResult<T>> GetOrSetVersionedItem<T>(Func<Task<T>> getter, Func<T, Task<long>> getVersion, 
            object key, long? currentVersion) where T : class;

        void RemoveVersionedCollection<T>(object key = null);

        void RemoveVersionedItem<T>(object instanceKey);

        void UpdateItem<T>(T item, object instanceKey);

        void UpdatedVersionedCollection<T>(IReadOnlyCollection<T> collection, long version, object key = null);

        void UpdatedVersionedItem<T>(T item, object instanceKey) where T : IVersioned;
    }
}
