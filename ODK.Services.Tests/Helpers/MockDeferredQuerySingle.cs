using System.Threading.Tasks;
using ODK.Core.Exceptions;
using ODK.Data.Core.Deferred;

namespace ODK.Services.Tests.Helpers;

internal class MockDeferredQuerySingle<T> : IDeferredQuerySingle<T>
    where T : class
{
    private readonly T? _value;

    public MockDeferredQuerySingle(T? value)
    {
        _value = value;
    }

    public Task<T> RunAsync()
    {
        if (_value == null)
        {
            throw new OdkNotFoundException();
        }

        return Task.FromResult(_value);
    }
}
