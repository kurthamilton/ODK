using System.Threading.Tasks;
using ODK.Data.Core.Deferred;

namespace ODK.Services.Tests.Helpers;

public class MockDeferredQuery<T> : IDeferredQuery<T>
{
    private readonly T _value;

    public MockDeferredQuery(T value)
    {
        _value = value;
    }

    public Task<T> Run() => Task.FromResult(_value);
}
