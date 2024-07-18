namespace ODK.Data.Core.Deferred;
public class DefaultSingleDeferredQuery<T> : IDeferredQuerySingle<T>
{
    private readonly T _value;

    public DefaultSingleDeferredQuery(T value)
    {
        _value = value;
    }

    public Task<T> RunAsync() => Task.FromResult(_value);
}
