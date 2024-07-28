using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Data.Core.Deferred;

namespace ODK.Services.Tests.Helpers;

internal class MockDeferredQueryMultiple<T> : IDeferredQueryMultiple<T>
    where T : class
{
    private readonly IEnumerable<T> _values;

    public MockDeferredQueryMultiple(IEnumerable<T>? values)
    {
        _values = values ?? [];
    }

    public Task<IReadOnlyCollection<T>> RunAsync()
        => Task.FromResult((IReadOnlyCollection<T>)_values.ToArray());
}
