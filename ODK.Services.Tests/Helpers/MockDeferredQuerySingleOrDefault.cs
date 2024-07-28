using System.Threading.Tasks;
using ODK.Data.Core.Deferred;

namespace ODK.Services.Tests.Helpers;

internal class MockDeferredQuerySingleOrDefault<T> : IDeferredQuerySingleOrDefault<T>
    where T : class
{
    private readonly T? _value;

    public MockDeferredQuerySingleOrDefault(T? value)
    {
        _value = value;
    }

    public Task<T?> RunAsync() => Task.FromResult(_value);
}
