using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Caching
{
    public interface ICache
    {
        Task<IReadOnlyCollection<T>> GetOrSetCollection<T>(Func<Task<IReadOnlyCollection<T>>> getter);

        void Remove<T>();

        IReadOnlyCollection<T> TryGetCollection<T>();
    }
}
