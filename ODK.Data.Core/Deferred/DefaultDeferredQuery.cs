namespace ODK.Data.Core.Deferred;

public class DefaultDeferredQuery<T> : IDeferredQuery<T>
{
    private readonly T _value;

    public DefaultDeferredQuery(T value)
    {
        _value = value;
    }

    public Task<T> Run() => Task.FromResult(_value);
}