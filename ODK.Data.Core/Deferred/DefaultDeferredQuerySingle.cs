namespace ODK.Data.Core.Deferred;

public class DefaultDeferredQuerySingle<T> : IDeferredQuerySingle<T>
{
    private readonly T _value;

    public DefaultDeferredQuerySingle(T value)
    {
        _value = value;
    }

    public Task<T> Run() => Task.FromResult(_value);
}
